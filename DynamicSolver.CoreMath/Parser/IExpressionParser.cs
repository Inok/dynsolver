using DynamicSolver.CoreMath.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Parser
{
    public interface IExpressionParser
    {
        [NotNull]
        IStatement Parse(string inputExpression);
    }
}