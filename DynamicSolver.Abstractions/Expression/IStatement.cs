using JetBrains.Annotations;

namespace DynamicSolver.Abstractions.Expression
{
    public interface IStatement
    {
        [NotNull]
        IExpression Expression { get; }

        bool IsEquation { get; }

        [NotNull]
        IEquation ToEquation();
    }
}