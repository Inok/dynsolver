using DynamicSolver.Abstractions.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.Expressions.Expression
{
    public sealed class SubtractBinaryOperator : BinaryOperator
    {
        public override string OperatorToken { get; } = "-";

        public SubtractBinaryOperator([NotNull] IExpression leftOperand, [NotNull] IExpression rightOperand) : base(leftOperand, rightOperand)
        {
        }
    }
}