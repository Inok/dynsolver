using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DynamicSolver.Expressions.Expression;
using DynamicSolver.Expressions.Tools;
using Inok.Tools.Dump;
using JetBrains.Annotations;

namespace DynamicSolver.Expressions.Execution.Interpreter
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

        public InterpretedFunction([NotNull] IStatement statement)
        {
            if (statement == null) throw new ArgumentNullException(nameof(statement));
            if (!statement.Analyzer.IsComputable) throw new ArgumentException("Expression is invalid: it is not computable.", nameof(statement));

            _expression = statement.Expression;

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
                    $"{arguments.DumpInline()} => {OrderedArguments.DumpInline()}");
            }

            var args = OrderedArguments.Select((a, i) => new {a, i}).ToDictionary(arg => arg.a, arg => arguments[arg.i]);
            return ExecuteInternal(_expression, args);
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
                    $"{arguments.Keys.DumpInline()} != {OrderedArguments.DumpInline()}");
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

                throw new InvalidOperationException($"Unknown primitive: {expression.Dump()}");
            }

            if (expression is IUnaryOperator)
            {
                var unaryMinus = expression as UnaryMinusOperator;
                if (unaryMinus != null)
                {
                    return -1.0*ExecuteInternal(unaryMinus.Operand, arguments);
                }

                throw new InvalidOperationException($"Unknown unary operator: {expression.Dump()}");
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
                Func<Func<double, double, double>, double> func = f => f(ExecuteInternal(binary.LeftOperand, arguments), ExecuteInternal(binary.RightOperand, arguments));
                switch (binary.OperatorToken)
                {
                    case "+":
                        return func((a, b) => a + b);
                    case "-":
                        return func((a, b) => a - b);
                    case "*":
                        return func((a, b) => a * b);
                    case "/":
                        return func((a, b) => a / b);
                    case "^":
                        return func(Math.Pow);
                    default:
                        throw new InvalidOperationException($"Unknown binary operator: {binary.Dump()}");
                }                
            }

            throw new InvalidOperationException($"Unknown expression: {expression.Dump()}");
        }
    }
}