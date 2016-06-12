using System.Collections.Generic;
using DynamicSolver.Expressions.Expression;

namespace DynamicSolver.Expressions.Execution
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