namespace DynamicSolver.Minimizer
{
    public interface IIterationLimitSettings
    {
        int MaxIterationCount { get; }
        bool AbortSearchOnIterationLimit { get; }
    }
}