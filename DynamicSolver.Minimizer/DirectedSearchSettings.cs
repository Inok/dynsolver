using System;
using JetBrains.Annotations;

namespace DynamicSolver.Minimizer
{
    public class DirectedSearchSettings : IIterationLimitSettings
    {
        [NotNull]
        public static readonly DirectedSearchSettings Default = new DirectedSearchSettings(100, 10e-8, true);

        public double Accuracy { get; }
        public int MaxIterationCount { get; }
        public bool AbortSearchOnIterationLimit { get; }

        public DirectedSearchSettings(int maxIterationCount, double accuracy, bool abortSearchOnIterationLimit)
        {
            if (maxIterationCount <= 0) throw new ArgumentOutOfRangeException(nameof(maxIterationCount));
            if (accuracy <= 0) throw new ArgumentOutOfRangeException(nameof(accuracy));

            MaxIterationCount = maxIterationCount;
            Accuracy = accuracy;
            AbortSearchOnIterationLimit = abortSearchOnIterationLimit;
        }
    }
}