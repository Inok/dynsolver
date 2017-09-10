using DynamicSolver.Core.Syntax.Model;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Syntax.Parser
{
    public interface ISyntaxParser
    {
        [NotNull]
        ISyntaxExpression Parse(string inputExpression);
    }
}