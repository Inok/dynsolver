using DynamicSolver.Abstractions;
using JetBrains.Annotations;

namespace DynamicSolver.LinearAlgebra.Derivative
{
    public interface IDerivativeCalculationStrategy
    {
        [NotNull]
        Vector Derivative([NotNull] IExecutableFunction function, [NotNull] Point point);

        double DerivativeByDirection([NotNull] IExecutableFunction function, [NotNull] Point point, [NotNull] Vector direction);
    }
}