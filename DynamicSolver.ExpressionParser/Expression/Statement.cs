using DynamicSolver.Abstractions.Expression;

namespace DynamicSolver.ExpressionParser.Expression
{
    public sealed class Statement : IStatement
    {
        public IExpression Expression { get; }

        public Statement(IExpression expression)
        {
            Expression = expression;
        }

        public override string ToString()
        {
            return Expression.ToString();
        }
    }
}