using System;
using DynamicSolver.LinearAlgebra;
using JetBrains.Annotations;

namespace DynamicSolver.Minimizer
{
    public class MinimizationResult
    {
        public IMultiDimensionalSearchStrategy Minimizer { get; }

        public bool Success { get; }
        public Point Minimum { get; }
        public double MinimumValue { get; }

        public MinimizationResult([NotNull] IMultiDimensionalSearchStrategy minimizer, bool success, Point minimum, double minimumValue)
        {
            if (minimizer == null) throw new ArgumentNullException(nameof(minimizer));

            Success = success;
            Minimum = minimum;
            MinimumValue = minimumValue;
            Minimizer = minimizer;
        }
    }
}