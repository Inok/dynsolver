using DynamicSolver.CoreMath.Syntax;
using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Parser
{
    public interface IExpressionParser
    {
        [NotNull]
        ISyntaxExpression Parse(string inputExpression);
    }
}