using System;
using JetBrains.Annotations;

namespace DynamicSolver.Minimizer
{
    public class MultiDimensionalSearchSettings : IIterationLimitSettings
    {
        [NotNull]
        public static readonly MultiDimensionalSearchSettings Default = new MultiDimensionalSearchSettings(10e-8, 100, true);

        public double Accuracy { get; }
        public int MaxIterationCount { get; }
        public bool AbortSearchOnIterationLimit { get; }

        public MultiDimensionalSearchSettings(double accuracy, int maxIterationCount, bool abortSearchOnIterationLimit)
        {
            if (maxIterationCount <= 0) throw new ArgumentOutOfRangeException(nameof(maxIterationCount));
            if (accuracy <= 0) throw new ArgumentOutOfRangeException(nameof(accuracy));

            MaxIterationCount = maxIterationCount;
            Accuracy = accuracy;
            AbortSearchOnIterationLimit = abortSearchOnIterationLimit;
        }
    }
}