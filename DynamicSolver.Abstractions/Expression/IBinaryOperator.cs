using JetBrains.Annotations;

namespace DynamicSolver.Abstractions.Expression
{
    public interface IBinaryOperator : IExpression
    {
        [NotNull]
        string OperatorToken { get; }
        [NotNull]
        IExpression LeftOperand { get; }
        [NotNull]
        IExpression RightOperand { get; }        
    }
}