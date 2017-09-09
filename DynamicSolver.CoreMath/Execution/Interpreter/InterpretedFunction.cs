using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DynamicSolver.CoreMath.Analysis;
using DynamicSolver.CoreMath.Syntax;
using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Execution.Interpreter
{
    public class InterpretedFunction : IExecutableFunction
    {
        private static readonly IDictionary<string, Func<double, double>> Functions = new Dictionary<string, Func<double, double>>
        {
            ["sin"] = (d) => Math.Sin(d),
            ["cos"] = (d) => Math.Cos(d),
            ["tg"]  = (d) => Math.Tan(d),
            ["ctg"] = (d) => 1.0 / Math.Tan(d),
            ["exp"] = (d) => Math.Exp(d),
            ["ln"]  = (d) => Math.Log(d),
            ["lg"]  = (d) => Math.Log10(d),
        };
        
        [NotNull] private readonly IExpression _expression;

        public IReadOnlyCollection<string> OrderedArguments { get; }

        public InterpretedFunction([NotNull] IExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            if (!new ExpressionAnalyzer(expression).IsComputable) throw new ArgumentException("Expression is invalid: it is not computable.", nameof(expression));

            _expression = expression;

            var list = new List<string>();

            var visitor = new ExpressionVisitor(_expression);
            visitor.VisitVariablePrimitive += (_, p) => list.Add(p.Name);
            visitor.VisitFunctionCall += (_, f) =>
            {
                if (!Functions.ContainsKey(f.FunctionName.ToLowerInvariant()))
                    throw new ArgumentException($"Expression contains not supported function call: {f.FunctionName}. " +
                                                $"Supported functions: {string.Join(", ", Functions.Keys)}");
            };
            visitor.Visit();

            OrderedArguments = new ReadOnlyCollection<string>(list.Distinct().OrderBy(s => s).ToList());
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

            var args = OrderedArguments.Select((a, i) => new {a, i}).ToDictionary(arg => arg.a, arg => arguments[arg.i]);
            return ExecuteInternal(_expression, args);
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

            return ExecuteInternal(_expression, arguments);
        }

        private double ExecuteInternal([NotNull] IExpression expression, [NotNull] IReadOnlyDictionary<string, double> arguments)
        {
            if (expression is IPrimitive)
            {
                var constantValue = (expression as ConstantPrimitive)?.Constant.Value;
                if (constantValue.HasValue)
                {
                    return constantValue.Value;
                }

                var numericValue = (expression as NumericPrimitive)?.Value;
                if (numericValue.HasValue)
                {
                    return numericValue.Value;
                }

                var variableName = (expression as VariablePrimitive)?.Name;
                if (variableName != null)
                {
                    return arguments[variableName];
                }

                throw new InvalidOperationException($"Unknown primitive {expression} of type {expression.GetType().FullName}");
            }

            if (expression is IUnaryOperator)
            {
                var unaryMinus = expression as UnaryMinusOperator;
                if (unaryMinus != null)
                {
                    return -1.0*ExecuteInternal(unaryMinus.Operand, arguments);
                }

                throw new InvalidOperationException($"Unknown unary operator {expression} of type {expression.GetType().FullName}");
            }

            var functionCall = expression as IFunctionCall;
            if (functionCall != null)
            {
                Func<double, double> funcImpl;
                if(!Functions.TryGetValue(functionCall.FunctionName.ToLowerInvariant(), out funcImpl))
                    throw new InvalidOperationException($"There is unknown function call: {functionCall.FunctionName}.");

                return funcImpl(ExecuteInternal(functionCall.Argument, arguments));
            }

            var binary = expression as IBinaryOperator;
            if (binary != null)
            {
                var leftValue = ExecuteInternal(binary.LeftOperand, arguments);
                var rightValue = ExecuteInternal(binary.RightOperand, arguments);

                switch (binary)
                {
                    case AddBinaryOperator _:
                        return leftValue + rightValue;
                    case SubtractBinaryOperator _:
                        return leftValue - rightValue;
                    case MultiplyBinaryOperator _:
                        return leftValue * rightValue;
                    case DivideBinaryOperator _:
                        return leftValue / rightValue;
                    case PowBinaryOperator _:
                        return Math.Pow(leftValue, rightValue);
                    default:
                        throw new InvalidOperationException($"Unknown binary operator {expression} of type {expression.GetType().FullName}");
                }                
            }

            throw new InvalidOperationException($"Unknown expression {expression} of type {expression.GetType().FullName}");
        }
    }
}