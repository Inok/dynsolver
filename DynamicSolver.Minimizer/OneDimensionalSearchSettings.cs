using System;
using JetBrains.Annotations;

namespace DynamicSolver.Minimizer
{
    public class OneDimensionalSearchSettings
    {
        [NotNull]
        public static readonly OneDimensionalSearchSettings Default = new OneDimensionalSearchSettings(100, 10e-8);

        public double Accuracy { get; }
        public int MaxStepCount { get; }

        public OneDimensionalSearchSettings(int maxStepCount, double accuracy)
        {
            if (maxStepCount <= 0) throw new ArgumentOutOfRangeException(nameof(maxStepCount));
            if (accuracy <= 0) throw new ArgumentOutOfRangeException(nameof(accuracy));

            MaxStepCount = maxStepCount;
            Accuracy = accuracy;
        }
    }
}