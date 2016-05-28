using JetBrains.Annotations;

namespace DynamicSolver.Expressions.Expression
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