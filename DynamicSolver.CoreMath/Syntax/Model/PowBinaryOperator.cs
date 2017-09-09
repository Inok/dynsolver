using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Syntax.Model
{
    public sealed class PowBinaryOperator : BinaryOperator
    {
        protected override string OperatorToken { get; } = "^";

        public PowBinaryOperator([NotNull] ISyntaxExpression leftOperand, [NotNull] ISyntaxExpression rightOperand) : base(leftOperand, rightOperand)
        {
        }
    }
}