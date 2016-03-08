using JetBrains.Annotations;

namespace DynamicSolver.Abstractions.Expression
{
    public interface IStatement
    {
        [NotNull]
        IExpression Expression { get; }

        [NotNull]
        IExpressionAnalyzer Analyzer { get; }
    }
}