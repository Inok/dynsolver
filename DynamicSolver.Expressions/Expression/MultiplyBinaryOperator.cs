using DynamicSolver.Abstractions.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.Expressions.Expression
{
    public sealed class MultiplyBinaryOperator : BinaryOperator
    {
        public override string OperatorToken { get; } = "*";

        public MultiplyBinaryOperator([NotNull] IExpression leftOperand, [NotNull] IExpression rightOperand) : base(leftOperand, rightOperand)
        {
        }
    }
}