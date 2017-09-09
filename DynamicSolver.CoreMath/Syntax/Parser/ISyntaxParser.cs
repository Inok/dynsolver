using DynamicSolver.CoreMath.Syntax.Model;
using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Syntax.Parser
{
    public interface ISyntaxParser
    {
        [NotNull]
        ISyntaxExpression Parse(string inputExpression);
    }
}