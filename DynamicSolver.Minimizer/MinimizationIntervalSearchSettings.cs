using System;
using JetBrains.Annotations;

namespace DynamicSolver.Minimizer
{
    public class MinimizationIntervalSearchSettings
    {
        [NotNull]
        public static readonly MinimizationIntervalSearchSettings Default = new MinimizationIntervalSearchSettings(10e-2, 100, 10e-8);

        public double InitialStepLength { get; }
        public int MaxStepCount { get; }
        public double DerivativeAccuracy { get; }

        public MinimizationIntervalSearchSettings(double initialStepLength, int maxStepCount, double derivativeAccuracy = 10e-8)
        {
            if (maxStepCount <= 0) throw new ArgumentOutOfRangeException(nameof(maxStepCount));
            if (derivativeAccuracy <= 0) throw new ArgumentOutOfRangeException(nameof(derivativeAccuracy));

            InitialStepLength = initialStepLength;
            MaxStepCount = maxStepCount;
            DerivativeAccuracy = derivativeAccuracy;
        }
    }
}