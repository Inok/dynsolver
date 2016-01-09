using System;
using DynamicSolver.Abstractions.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.ExpressionParser.Expression
{
    public sealed class FunctionCall : IFunctionCall
    {
        public string FunctionName { get; }

        public IExpression Argument { get; set; }

        public FunctionCall(string functionName, [NotNull] IExpression argument)
        {
            if (string.IsNullOrEmpty(functionName)) throw new ArgumentException("Argument is null or empty", nameof(functionName));
            if (argument == null) throw new ArgumentNullException(nameof(argument));

            FunctionName = functionName;
            Argument = argument;
        }

        public override string ToString()
        {
            return $"{FunctionName}({(Argument as IBinaryOperator)?.ToString(false) ?? Argument.ToString()})";
        }
    }
}