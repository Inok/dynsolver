using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Expression
{
    public sealed class AddBinaryOperator : BinaryOperator
    {
        protected override string OperatorToken { get; } = "+";

        public AddBinaryOperator([NotNull] IExpression leftOperand, [NotNull] IExpression rightOperand) : base(leftOperand, rightOperand)
        {
        }
    }
}