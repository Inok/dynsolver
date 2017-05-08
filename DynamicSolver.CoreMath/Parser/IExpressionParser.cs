using DynamicSolver.CoreMath.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Parser
{
    public interface IExpressionParser
    {
        [NotNull]
        IExpression Parse(string inputExpression);
    }
}