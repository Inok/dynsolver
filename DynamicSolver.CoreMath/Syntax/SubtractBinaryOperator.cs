using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Syntax
{
    public sealed class SubtractBinaryOperator : BinaryOperator
    {
        protected override string OperatorToken { get; } = "-";

        public SubtractBinaryOperator([NotNull] IExpression leftOperand, [NotNull] IExpression rightOperand) : base(leftOperand, rightOperand)
        {
        }
    }
}