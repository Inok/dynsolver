using DynamicSolver.Abstractions.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.ExpressionParser.Expression
{
    public sealed class EqualityBinaryOperator : BinaryOperator
    {
        public override string OperatorToken { get; } = "=";

        public EqualityBinaryOperator([NotNull] IExpression leftOperand, [NotNull] IExpression rightOperand) : base(leftOperand, rightOperand)
        {
        }
    }
}