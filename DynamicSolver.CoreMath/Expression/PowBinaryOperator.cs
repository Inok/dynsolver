using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Expression
{
    public sealed class PowBinaryOperator : BinaryOperator
    {
        protected override string OperatorToken { get; } = "^";

        public PowBinaryOperator([NotNull] IExpression leftOperand, [NotNull] IExpression rightOperand) : base(leftOperand, rightOperand)
        {
        }
    }
}