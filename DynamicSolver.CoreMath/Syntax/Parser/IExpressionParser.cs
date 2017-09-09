using DynamicSolver.CoreMath.Syntax.Model;
using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Syntax.Parser
{
    public interface IExpressionParser
    {
        [NotNull]
        ISyntaxExpression Parse(string inputExpression);
    }
}