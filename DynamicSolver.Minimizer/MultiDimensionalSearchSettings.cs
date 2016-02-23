using System;
using JetBrains.Annotations;

namespace DynamicSolver.Minimizer
{
    public class MultiDimensionalSearchSettings
    {
        [NotNull]
        public static readonly MultiDimensionalSearchSettings Default = new MultiDimensionalSearchSettings(100, 10e-8);

        public double Accuracy { get; }
        public int MaxStepCount { get; }

        public MultiDimensionalSearchSettings(int maxStepCount, double accuracy)
        {
            if (maxStepCount <= 0) throw new ArgumentOutOfRangeException(nameof(maxStepCount));
            if (accuracy <= 0) throw new ArgumentOutOfRangeException(nameof(accuracy));

            MaxStepCount = maxStepCount;
            Accuracy = accuracy;
        }
    }
}