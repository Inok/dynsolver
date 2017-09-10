using JetBrains.Annotations;

namespace DynamicSolver.Core.Syntax.Model
{
    public sealed class DivideBinaryOperator : BinaryOperator
    {
        protected override string OperatorToken { get; } = "/";

        public DivideBinaryOperator([NotNull] ISyntaxExpression leftOperand, [NotNull] ISyntaxExpression rightOperand) : base(leftOperand, rightOperand)
        {
        }
    }
}