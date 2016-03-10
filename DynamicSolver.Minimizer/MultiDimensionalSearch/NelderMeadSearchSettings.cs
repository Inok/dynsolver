using System;
using JetBrains.Annotations;

namespace DynamicSolver.Minimizer.MultiDimensionalSearch
{
    public class NelderMeadSearchSettings : MultiDimensionalSearchSettings, IIterationLimitSettings
    {
        [NotNull]
        public new static readonly NelderMeadSearchSettings Default = new NelderMeadSearchSettings(10e-8, 100, true, 1, 0.5, 2);

        public double ReflectionCoefficient { get; }
        public double CompressionCoefficient { get; }
        public double ExpansionCoefficient { get; }

        public NelderMeadSearchSettings(double accuracy, int maxIterationCount, bool abortSearchOnIterationLimit, double reflectionCoefficient, double compressionCoefficient, double expansionCoefficient) 
            : base(accuracy, maxIterationCount, abortSearchOnIterationLimit)
        {
            if (reflectionCoefficient <= 0) throw new ArgumentOutOfRangeException(nameof(reflectionCoefficient));
            if (compressionCoefficient <= 0 || compressionCoefficient > 1) throw new ArgumentOutOfRangeException(nameof(compressionCoefficient));
            if (expansionCoefficient <= 1) throw new ArgumentOutOfRangeException(nameof(compressionCoefficient));

            ReflectionCoefficient = reflectionCoefficient;
            CompressionCoefficient = compressionCoefficient;
            ExpansionCoefficient = expansionCoefficient;
        }
    }
}