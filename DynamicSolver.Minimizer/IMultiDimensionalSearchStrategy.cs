using DynamicSolver.Abstractions;
using DynamicSolver.LinearAlgebra;
using JetBrains.Annotations;

namespace DynamicSolver.Minimizer
{
    public interface IMultiDimensionalSearchStrategy
    {
        Point Search([NotNull] IExecutableFunction function, [NotNull] Point startPoint);
    }
}