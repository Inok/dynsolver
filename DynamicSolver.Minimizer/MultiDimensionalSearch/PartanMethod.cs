using System;
using System.Linq;
using DynamicSolver.Abstractions;
using DynamicSolver.LinearAlgebra;
using DynamicSolver.LinearAlgebra.Derivative;
using JetBrains.Annotations;
using Microsoft.VisualBasic.CompilerServices;

namespace DynamicSolver.Minimizer.MultiDimensionalSearch
{
    public class PartanMethod : IMultiDimensionalSearchStrategy
    {
        private readonly IDirectedSearchStrategy _directedSearchStrategy;
        private readonly IDerivativeCalculationStrategy _derivativeCalculator;
        private readonly MultiDimensionalSearchSettings _settings;

        public PartanMethod(
            [NotNull] IDirectedSearchStrategy directedSearchStrategy, 
            [NotNull] IDerivativeCalculationStrategy derivativeCalculator,
            [NotNull] MultiDimensionalSearchSettings settings)
        {
            if (directedSearchStrategy == null) throw new ArgumentNullException(nameof(directedSearchStrategy));
            if (derivativeCalculator == null) throw new ArgumentNullException(nameof(derivativeCalculator));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            _directedSearchStrategy = directedSearchStrategy;
            _derivativeCalculator = derivativeCalculator;
            _settings = settings;
        }

        public Point Search(IExecutableFunction function, Point startPoint)
        {
            var x1 = startPoint;
            var x2 = _directedSearchStrategy.SearchInterval(function, x1, _derivativeCalculator.Derivative(function, startPoint)).Center;

            int iteration = 0;
            do
            {
                var x3 = _directedSearchStrategy.SearchInterval(function, x2, _derivativeCalculator.Derivative(function, x2)).Center;
                var x4 = _directedSearchStrategy.SearchInterval(function, x3, new Vector(x1, x3)).Center;

                if (x4.Any(d => double.IsNaN(d) || double.IsInfinity(d)))
                {
                    return x1;
                }

                if (new Vector(x1, x4).Length < _settings.Accuracy)
                {
                    return x4;
                }

                x1 = x2;
                x2 = x4;

                iteration++;
            }
            while (iteration < _settings.MaxStepCount);

            throw new InvalidOperationException($"Search was interrupted because iteration limit has been reached: {iteration}.");
        }
    }
}