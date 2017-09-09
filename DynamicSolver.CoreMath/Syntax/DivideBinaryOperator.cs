using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Syntax
{
    public sealed class DivideBinaryOperator : BinaryOperator
    {
        protected override string OperatorToken { get; } = "/";

        public DivideBinaryOperator([NotNull] IExpression leftOperand, [NotNull] IExpression rightOperand) : base(leftOperand, rightOperand)
        {
        }
    }
}