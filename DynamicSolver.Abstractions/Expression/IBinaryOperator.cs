namespace DynamicSolver.Abstractions.Expression
{
    public interface IBinaryOperator : IExpression
    {
        string OperatorToken { get; }
        IExpression LeftOperand { get; }
        IExpression RightOperand { get; }
        string ToString(bool addParentheses);
    }
}