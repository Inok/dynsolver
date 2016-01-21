using DynamicSolver.Abstractions.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.ExpressionParser.Parser
{
    public interface IExpressionParser
    {
        [NotNull]
        IStatement Parse(string inputExpression);
    }
}