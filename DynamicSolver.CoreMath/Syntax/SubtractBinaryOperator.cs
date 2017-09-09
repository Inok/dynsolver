using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Syntax
{
    public sealed class SubtractBinaryOperator : BinaryOperator
    {
        protected override string OperatorToken { get; } = "-";

        public SubtractBinaryOperator([NotNull] ISyntaxExpression leftOperand, [NotNull] ISyntaxExpression rightOperand) : base(leftOperand, rightOperand)
        {
        }
    }
}