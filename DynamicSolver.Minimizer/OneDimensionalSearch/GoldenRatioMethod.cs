using System;
using System.Linq;
using DynamicSolver.Abstractions;
using DynamicSolver.LinearAlgebra;
using JetBrains.Annotations;

namespace DynamicSolver.Minimizer.OneDimensionalSearch
{
    public class GoldenRatioMethod : IDirectedSearchStrategy
    {
        private static readonly double Tau1 = (Math.Sqrt(5) - 1) / 2;
        private static readonly double Tau2 = 1 - Tau1;

        private readonly DirectedSearchSettings _settings;
        private readonly IDirectedSearchStrategy _intervalSearchStrategy;

        public GoldenRatioMethod([NotNull] IDirectedSearchStrategy intervalSearchStrategy, [NotNull] DirectedSearchSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (intervalSearchStrategy == null) throw new ArgumentNullException(nameof(intervalSearchStrategy));

            _settings = settings;
            _intervalSearchStrategy = intervalSearchStrategy;
        }

        public Interval SearchInterval(IExecutableFunction function, Point startPoint, Vector direction)
        {
            var interval = _intervalSearchStrategy.SearchInterval(function, startPoint, direction);
            direction = interval.Direction;

            double a = 0;
            var b = interval.Length;

            var lambda = a + Tau2 * Math.Abs(b - a);
            var mu = a + Tau1 * Math.Abs(b - a);

            var iteration = 0;
            do
            {
                if (function.Execute(interval.First.Move(direction, lambda).ToArray()) < function.Execute(interval.First.Move(direction, mu).ToArray()))
                {
                    b = mu;
                    mu = lambda;
                    lambda = a + Tau2 * Math.Abs(b - a);
                }
                else
                {
                    a = lambda;
                    lambda = mu;
                    mu = a + Tau1 * Math.Abs(b - a);
                }

                if(Math.Abs(b - a) <= _settings.Accuracy)
                    return new Interval(interval.First.Move(direction, a), interval.First.Move(direction, b));

                iteration++;
            }
            while (iteration < _settings.MaxStepCount);

            throw new InvalidOperationException($"Search was interrupted because iteration limit has been reached: {iteration}.");
        }
    }
}