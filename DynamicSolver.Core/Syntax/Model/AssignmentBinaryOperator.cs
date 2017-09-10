using JetBrains.Annotations;

namespace DynamicSolver.Core.Syntax.Model
{
    public sealed class AssignmentBinaryOperator : BinaryOperator
    {
        protected override string OperatorToken { get; } = "=";

        public AssignmentBinaryOperator([NotNull] ISyntaxExpression leftOperand, [NotNull] ISyntaxExpression rightOperand) : base(leftOperand, rightOperand)
        {
        }
    }
}