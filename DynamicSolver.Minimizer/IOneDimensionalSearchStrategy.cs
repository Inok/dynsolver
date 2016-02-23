using DynamicSolver.Abstractions;
using DynamicSolver.LinearAlgebra;
using JetBrains.Annotations;

namespace DynamicSolver.Minimizer
{
    public interface IOneDimensionalSearchStrategy
    {
        Interval SearchInterval([NotNull] IExecutableFunction function, [NotNull] Point startPoint, [NotNull] Vector direction);
    }
}