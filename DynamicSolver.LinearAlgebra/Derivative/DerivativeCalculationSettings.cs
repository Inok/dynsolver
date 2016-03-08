using System;

namespace DynamicSolver.LinearAlgebra.Derivative
{
    public class DerivativeCalculationSettings
    {
        public static readonly DerivativeCalculationSettings Default = new DerivativeCalculationSettings(10e-8, DerivativeMode.Auto);

        public DerivativeMode Mode { get; }
        public double Increment { get; }

        public DerivativeCalculationSettings(double increment, DerivativeMode mode)
        {
            if (increment == 0) throw new ArgumentOutOfRangeException(nameof(increment), "Increment should be not zero.");

            Increment = increment;
            Mode = mode;
        }
    }
}