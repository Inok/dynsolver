using System.Collections.Generic;
using DynamicSolver.CoreMath.Syntax;

namespace DynamicSolver.CoreMath.Analysis
{
    public interface IExpressionAnalyzer
    {
        IReadOnlyCollection<string> Variables { get; }
        ISet<string> GetVariablesSet();

        bool IsSimpleAssignment { get; }
        bool IsComputable { get; }

        bool HasOperator<T>() where T : IExpression;
    }
}