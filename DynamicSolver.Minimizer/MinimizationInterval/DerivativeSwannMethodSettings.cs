using System;
using JetBrains.Annotations;

namespace DynamicSolver.Minimizer.MinimizationInterval
{
    public class DerivativeSwannMethodSettings
    {
        [NotNull]
        public static readonly DerivativeSwannMethodSettings Default = new DerivativeSwannMethodSettings(10e-2, 100, 10e-8);

        public double InitialStepLength { get; }
        public int MaxStepCount { get; }
        public double DerivativeAccuracy { get; }

        public DerivativeSwannMethodSettings(double initialStepLength, int maxStepCount, double derivativeAccuracy = 10e-8)
        {
            if (maxStepCount <= 0) throw new ArgumentOutOfRangeException(nameof(maxStepCount));
            if (derivativeAccuracy <= 0) throw new ArgumentOutOfRangeException(nameof(derivativeAccuracy));

            InitialStepLength = initialStepLength;
            MaxStepCount = maxStepCount;
            DerivativeAccuracy = derivativeAccuracy;
        }
    }
}