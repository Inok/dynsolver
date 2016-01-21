using JetBrains.Annotations;

namespace DynamicSolver.Abstractions
{
    public interface IFunction
    {
        double Execute([NotNull] double[] arguments);
    }
}