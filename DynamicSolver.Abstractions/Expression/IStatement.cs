using System;
using JetBrains.Annotations;

namespace DynamicSolver.Abstractions.Expression
{
    public interface IStatement : IEquatable<IStatement>
    {
        [NotNull]
        IExpression Expression { get; }

        [NotNull]
        IExpressionAnalyzer Analyzer { get; }                
    }
}