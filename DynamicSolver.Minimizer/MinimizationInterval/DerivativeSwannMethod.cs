using System;
using DynamicSolver.Abstractions;
using DynamicSolver.LinearAlgebra;
using DynamicSolver.LinearAlgebra.Derivative;
using JetBrains.Annotations;

namespace DynamicSolver.Minimizer.MinimizationInterval
{
    public class DerivativeSwannMethod : IDirectedSearchStrategy
    {
        private readonly DerivativeSwannMethodSettings _settings;
        private readonly IDerivativeCalculationStrategy _derivativeStrategy;

        public DerivativeSwannMethod([NotNull] DerivativeSwannMethodSettings settings, IDerivativeCalculationStrategy derivativeStrategy)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            _settings = settings;
            _derivativeStrategy = derivativeStrategy;
        }

        public Interval SearchInterval(IExecutableFunction function, Point startPoint, Vector direction)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));
            if (startPoint == null) throw new ArgumentNullException(nameof(startPoint));
            if (direction == null) throw new ArgumentNullException(nameof(direction));
            if (function.OrderedArguments.Count != startPoint.Dimension)
                throw new ArgumentException($"Function has {function.OrderedArguments.Count} arguments, but start point has dimension of {startPoint.Dimension}");
            if (function.OrderedArguments.Count != direction.Dimension)
                throw new ArgumentException($"Function has {function.OrderedArguments.Count} arguments, but direction has dimension of {direction.Dimension}");
            
            double step = _settings.InitialStepLength;

            if (_derivativeStrategy.DerivativeByDirection(function, startPoint, direction) > 0)
                step = -step;

            double shift = 0;
            var iteration = 0;
            do
            {
                var first = startPoint.Move(direction, shift);
                var second = startPoint.Move(direction, shift + step);

                var firstValue = _derivativeStrategy.DerivativeByDirection(function, first, direction);
                var secondValue = _derivativeStrategy.DerivativeByDirection(function, second, direction);
                if(firstValue * secondValue <= 0 || firstValue < _settings.DerivativeAccuracy || secondValue < _settings.DerivativeAccuracy)
                    return new Interval(first, second);

                shift += step;
                step *= 2;
                iteration++;
            }
            while (iteration < _settings.MaxStepCount);
            
            throw new InvalidOperationException($"Search was interrupted because iteration limit has been reached: {iteration}.");
        }
    }
}