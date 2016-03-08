using System;
using JetBrains.Annotations;

namespace DynamicSolver.Minimizer
{
    public class HookeJeevesSearchSettings : MultiDimensionalSearchSettings
    {
        [NotNull]
        public new static readonly HookeJeevesSearchSettings Default = new HookeJeevesSearchSettings(10e-8, 100, true, 1, 0.5);

        public double InitialIncrement { get; }
        public double StepReductionFactor { get; }

        public HookeJeevesSearchSettings(double accuracy, int maxIterationCount, bool abortSearchOnIterationLimit, double initialIncrement, double stepReductionFactor) 
            : base(accuracy, maxIterationCount, abortSearchOnIterationLimit)
        {
            if (initialIncrement <= 0) throw new ArgumentOutOfRangeException(nameof(initialIncrement));
            if (stepReductionFactor <= 0 || stepReductionFactor >= 1) throw new ArgumentOutOfRangeException(nameof(stepReductionFactor));

            InitialIncrement = initialIncrement;
            StepReductionFactor = stepReductionFactor;
        }
    }
}