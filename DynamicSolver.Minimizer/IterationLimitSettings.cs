namespace DynamicSolver.Minimizer
{
    internal class IterationLimitSettings : IIterationLimitSettings
    {
        public int MaxIterationCount { get; }
        public bool AbortSearchOnIterationLimit { get; }

        public IterationLimitSettings(int maxIterationCount, bool abortSearchOnIterationLimit)
        {
            MaxIterationCount = maxIterationCount;
            AbortSearchOnIterationLimit = abortSearchOnIterationLimit;
        }
    }
}