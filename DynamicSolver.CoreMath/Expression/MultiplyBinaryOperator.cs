using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Expression
{
    public sealed class MultiplyBinaryOperator : BinaryOperator
    {
        protected override string OperatorToken { get; } = "*";

        public MultiplyBinaryOperator([NotNull] IExpression leftOperand, [NotNull] IExpression rightOperand) : base(leftOperand, rightOperand)
        {
        }
    }
}