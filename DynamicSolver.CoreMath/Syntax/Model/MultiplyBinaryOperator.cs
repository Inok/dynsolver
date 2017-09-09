using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Syntax.Model
{
    public sealed class MultiplyBinaryOperator : BinaryOperator
    {
        protected override string OperatorToken { get; } = "*";

        public MultiplyBinaryOperator([NotNull] ISyntaxExpression leftOperand, [NotNull] ISyntaxExpression rightOperand) : base(leftOperand, rightOperand)
        {
        }
    }
}