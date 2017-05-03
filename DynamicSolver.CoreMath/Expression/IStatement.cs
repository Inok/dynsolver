using System;
using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Expression
{
    public interface IStatement : IEquatable<IStatement>
    {
        [NotNull]
        IExpression Expression { get; }                
    }
}