using JetBrains.Annotations;

namespace DynamicSolver.Abstractions.Expression
{
    public interface IInitialState
    {
        [NotNull]
        IVariablePrimitive Variable { get; }

        [NotNull]
        IValuePrimitive Value { get; }
    }
}