using System;
using JetBrains.Annotations;

namespace DynamicSolver.Expressions.Expression
{
    public interface IStatement : IEquatable<IStatement>
    {
        [NotNull]
        IExpression Expression { get; }                
    }
}