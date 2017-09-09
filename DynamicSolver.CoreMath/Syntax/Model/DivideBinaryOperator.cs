using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Syntax.Model
{
    public sealed class DivideBinaryOperator : BinaryOperator
    {
        protected override string OperatorToken { get; } = "/";

        public DivideBinaryOperator([NotNull] ISyntaxExpression leftOperand, [NotNull] ISyntaxExpression rightOperand) : base(leftOperand, rightOperand)
        {
        }
    }
}