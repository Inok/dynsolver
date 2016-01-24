using JetBrains.Annotations;

namespace DynamicSolver.Abstractions.Expression
{
    public interface IEquation
    {
        [NotNull]
        IVariablePrimitive Variable { get; }

        [NotNull]
        IExpression Expression { get; }
    }
}