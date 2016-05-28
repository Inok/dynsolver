using System;
using DynamicSolver.Expressions.Execution;
using JetBrains.Annotations;

namespace DynamicSolver.Expressions.Expression
{
    public interface IStatement : IEquatable<IStatement>
    {
        [NotNull]
        IExpression Expression { get; }

        [NotNull]
        IExpressionAnalyzer Analyzer { get; }                
    }
}