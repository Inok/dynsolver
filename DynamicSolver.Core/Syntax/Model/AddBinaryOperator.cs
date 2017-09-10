using JetBrains.Annotations;

namespace DynamicSolver.Core.Syntax.Model
{
    public sealed class AddBinaryOperator : BinaryOperator
    {
        protected override string OperatorToken { get; } = "+";

        public AddBinaryOperator([NotNull] ISyntaxExpression leftOperand, [NotNull] ISyntaxExpression rightOperand) : base(leftOperand, rightOperand)
        {
        }
    }
}