using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using DynamicSolver.CoreMath.Analysis;
using DynamicSolver.CoreMath.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Execution.Compiler
{
    public class CompiledFunction : IExecutableFunction
    {
        [NotNull] private readonly Func<double[], double> _function;
        
        public IReadOnlyCollection<string> OrderedArguments { get; }

        public CompiledFunction([NotNull] IExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var analyzer = new ExpressionAnalyzer(expression);

            if (!analyzer.IsComputable)
            {
                throw new ArgumentException("Expression is invalid: it is not computable.", nameof(expression));
            }

            var arguments = analyzer.Variables.OrderBy(s => s).ToList();

            _function = BuildFunction(expression, arguments);
            OrderedArguments = new ReadOnlyCollection<string>(arguments);
        }

        public double Execute(IReadOnlyDictionary<string, double> arguments)
        {
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));

            var set = new HashSet<string>(arguments.Keys);
            set.SymmetricExceptWith(OrderedArguments);
            if (set.Count > 0)
            {
                throw new ArgumentException(
                    $"There is mismatch between arguments and expression: " +
                    $"{string.Join(", ", arguments.Keys)} != {string.Join(", ", OrderedArguments)}");
            }

            return _function(OrderedArguments.Select(a => arguments[a]).ToArray());
        }

        public double Execute(double[] arguments)
        {
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));

            if (arguments.Length != OrderedArguments.Count)
            {
                throw new ArgumentException(
                    $"There is mismatch between count of arguments and arguments of expression: " +
                    $"{string.Join(", ", arguments)} => {string.Join(", ", OrderedArguments)}");
            }

            return _function(arguments);
        }

        private Func<double[], double> BuildFunction(IExpression expression, List<string> orderedArguments)
        {
            var argumentsParameter = System.Linq.Expressions.Expression.Parameter(typeof(double[]), "args");
            var lambdaExpression = System.Linq.Expressions.Expression.Lambda(BuildExpression(expression, orderedArguments, argumentsParameter), argumentsParameter);
            return (Func<double[], double>) lambdaExpression.Compile();
        }

        private System.Linq.Expressions.Expression BuildExpression(IExpression expression, IList<string> orderedArguments, ParameterExpression argumentsParameter)
        {
            if (expression is IPrimitive)
            {
                var constantValue = (expression as ConstantPrimitive)?.Constant;
                if (constantValue != null)
                {
                    return System.Linq.Expressions.Expression.Constant(constantValue.Value);
                }

                var numericValue = (expression as NumericPrimitive)?.Value;
                if (numericValue.HasValue)
                {
                    return System.Linq.Expressions.Expression.Constant(numericValue.Value);
                }

                var variableName = (expression as VariablePrimitive)?.Name;
                if (variableName != null)
                {
                    return System.Linq.Expressions.Expression.ArrayIndex(argumentsParameter, System.Linq.Expressions.Expression.Constant(orderedArguments.IndexOf(variableName)));
                }

                throw new InvalidOperationException($"Unknown primitive {expression} of type {expression.GetType().FullName}");
            }

            if (expression is IUnaryOperator)
            {
                var unaryMinus = expression as UnaryMinusOperator;
                if (unaryMinus != null)
                {
                    return System.Linq.Expressions.Expression.Multiply(System.Linq.Expressions.Expression.Constant(-1d), BuildExpression(unaryMinus.Operand, orderedArguments, argumentsParameter));
                }

                throw new InvalidOperationException($"Unknown unary operator {expression} of type {expression.GetType().FullName}");
            }

            var functionCall = expression as IFunctionCall;
            if (functionCall != null)
            {
                switch (functionCall.FunctionName.ToLowerInvariant())
                {
                    case "sin": return System.Linq.Expressions.Expression.Call(null, typeof(Math).GetMethod("Sin"), BuildExpression(functionCall.Argument, orderedArguments, argumentsParameter));
                    case "cos": return System.Linq.Expressions.Expression.Call(null, typeof(Math).GetMethod("Cos"), BuildExpression(functionCall.Argument, orderedArguments, argumentsParameter));
                    case "tg":  return System.Linq.Expressions.Expression.Call(null, typeof(Math).GetMethod("Tan"), BuildExpression(functionCall.Argument, orderedArguments, argumentsParameter));
                    case "ctg": return System.Linq.Expressions.Expression.Divide(System.Linq.Expressions.Expression.Constant(1d), System.Linq.Expressions.Expression.Call(null, typeof(Math).GetMethod("Tan"), BuildExpression(functionCall.Argument, orderedArguments, argumentsParameter)));
                    case "exp": return System.Linq.Expressions.Expression.Call(null, typeof(Math).GetMethod("Exp"), BuildExpression(functionCall.Argument, orderedArguments, argumentsParameter));
                    case "ln":  return System.Linq.Expressions.Expression.Call(null, typeof(Math).GetMethod("Log", new [] {typeof(double)}), BuildExpression(functionCall.Argument, orderedArguments, argumentsParameter));
                    case "lg":  return System.Linq.Expressions.Expression.Call(null, typeof(Math).GetMethod("Log10"), BuildExpression(functionCall.Argument, orderedArguments, argumentsParameter));
                    default:
                        throw new ArgumentException($"Expression contains not supported function call: {functionCall.FunctionName}");
                }
            }

            var binary = expression as IBinaryOperator;
            if (binary != null)
            {
                var left = BuildExpression(binary.LeftOperand, orderedArguments, argumentsParameter);
                var right = BuildExpression(binary.RightOperand, orderedArguments, argumentsParameter);
                switch (binary)
                {
                    case AddBinaryOperator _:
                        return System.Linq.Expressions.Expression.Add(left, right);
                    case SubtractBinaryOperator _:
                        return System.Linq.Expressions.Expression.Subtract(left, right);
                    case MultiplyBinaryOperator _:
                        return System.Linq.Expressions.Expression.Multiply(left, right);
                    case DivideBinaryOperator _:
                        return System.Linq.Expressions.Expression.Divide(left, right);
                    case PowBinaryOperator _:
                        return System.Linq.Expressions.Expression.Call(null, typeof(Math).GetMethod("Pow"), left, right);
                    default:
                        throw new InvalidOperationException($"Unknown binary operator {expression} of type {expression.GetType().FullName}");
                }
            }
            throw new InvalidOperationException($"Unknown expression {expression} of type {expression.GetType().FullName}");
        }
    }
}