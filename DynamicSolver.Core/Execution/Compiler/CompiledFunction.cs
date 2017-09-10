using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DynamicSolver.Core.Syntax;
using DynamicSolver.Core.Syntax.Model;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Execution.Compiler
{
    public class CompiledFunction : IExecutableFunction
    {
        [NotNull] private readonly Func<double[], double> _function;
        
        public IReadOnlyCollection<string> OrderedArguments { get; }

        public CompiledFunction([NotNull] ISyntaxExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var analyzer = new SyntaxExpressionAnalyzer(expression);

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

            var set = new HashSet<string>(OrderedArguments);
            set.ExceptWith(arguments.Keys);
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

        private Func<double[], double> BuildFunction(ISyntaxExpression expression, List<string> orderedArguments)
        {
            var argumentsParameter = Expression.Parameter(typeof(double[]), "args");
            var lambdaExpression = Expression.Lambda(BuildExpression(expression, orderedArguments, argumentsParameter), argumentsParameter);
            return (Func<double[], double>) lambdaExpression.Compile();
        }

        private Expression BuildExpression(ISyntaxExpression expression, IList<string> orderedArguments, ParameterExpression argumentsParameter)
        {
            if (expression is IPrimitive)
            {
                if (expression is ConstantPrimitive constantValue)
                {
                    return Expression.Constant(constantValue.Constant.Value);
                }

                if (expression is NumericPrimitive numericValue)
                {
                    return Expression.Constant(numericValue.Value);
                }

                if (expression is VariablePrimitive variableName)
                {
                    return Expression.ArrayIndex(argumentsParameter, Expression.Constant(orderedArguments.IndexOf(variableName.Name)));
                }

                throw new InvalidOperationException($"Unknown primitive {expression} of type {expression.GetType().FullName}");
            }

            if (expression is IUnaryOperator)
            {
                if (expression is UnaryMinusOperator unaryMinus)
                {
                    return Expression.Multiply(Expression.Constant(-1d), BuildExpression(unaryMinus.Operand, orderedArguments, argumentsParameter));
                }

                throw new InvalidOperationException($"Unknown unary operator {expression} of type {expression.GetType().FullName}");
            }

            var mathType = typeof(Math).GetTypeInfo();
            
            if (expression is IFunctionCall functionCall)
            {
                switch (functionCall.FunctionName.ToLowerInvariant())
                {
                    case "sin": return Expression.Call(null, mathType.GetDeclaredMethod(nameof(Math.Sin)), BuildExpression(functionCall.Argument, orderedArguments, argumentsParameter));
                    case "cos": return Expression.Call(null, mathType.GetDeclaredMethod(nameof(Math.Cos)), BuildExpression(functionCall.Argument, orderedArguments, argumentsParameter));
                    case "tg":  return Expression.Call(null, mathType.GetDeclaredMethod(nameof(Math.Tan)), BuildExpression(functionCall.Argument, orderedArguments, argumentsParameter));
                    case "ctg":
                        return Expression.Divide(
                            Expression.Constant(1d),
                            Expression.Call(null, mathType.GetDeclaredMethod(nameof(Math.Tan)), BuildExpression(functionCall.Argument, orderedArguments, argumentsParameter)));
                    case "exp": return Expression.Call(null, mathType.GetDeclaredMethod(nameof(Math.Exp)), BuildExpression(functionCall.Argument, orderedArguments, argumentsParameter));
                    case "ln":  return Expression.Call(null, mathType.GetDeclaredMethods(nameof(Math.Log)).First(m => m.GetParameters().Length == 1), BuildExpression(functionCall.Argument, orderedArguments, argumentsParameter));
                    case "lg":  return Expression.Call(null, mathType.GetDeclaredMethod(nameof(Math.Log10)), BuildExpression(functionCall.Argument, orderedArguments, argumentsParameter));
                    default:
                        throw new ArgumentException($"Expression contains not supported function call: {functionCall.FunctionName}");
                }
            }

            if (expression is IBinaryOperator binary)
            {
                var left = BuildExpression(binary.LeftOperand, orderedArguments, argumentsParameter);
                var right = BuildExpression(binary.RightOperand, orderedArguments, argumentsParameter);
                switch (binary)
                {
                    case AddBinaryOperator _:
                        return Expression.Add(left, right);
                    case SubtractBinaryOperator _:
                        return Expression.Subtract(left, right);
                    case MultiplyBinaryOperator _:
                        return Expression.Multiply(left, right);
                    case DivideBinaryOperator _:
                        return Expression.Divide(left, right);
                    case PowBinaryOperator _:
                        return Expression.Call(null, mathType.GetDeclaredMethod(nameof(Math.Pow)), left, right);
                    default:
                        throw new InvalidOperationException($"Unknown binary operator {expression} of type {expression.GetType().FullName}");
                }
            }
            throw new InvalidOperationException($"Unknown expression {expression} of type {expression.GetType().FullName}");
        }
    }
}