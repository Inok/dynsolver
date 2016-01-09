using System;
using DynamicSolver.Abstractions.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.ExpressionParser.Expression
{
    public sealed class UnaryMinusOperator : IUnaryOperator
    {
        public IExpression Operand { get; set; }

        public UnaryMinusOperator([NotNull] IExpression operand)
        {
            if (operand == null) throw new ArgumentNullException(nameof(operand));

            Operand = operand;
        }

        public override string ToString()
        {
            return $"-{Operand}";
        }
    }
}