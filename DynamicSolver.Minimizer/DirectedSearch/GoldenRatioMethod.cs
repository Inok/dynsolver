using System;
using System.Linq;
using DynamicSolver.Abstractions;
using DynamicSolver.LinearAlgebra;
using JetBrains.Annotations;

namespace DynamicSolver.Minimizer.DirectedSearch
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
            if (function == null) throw new ArgumentNullException(nameof(function));
            if (startPoint == null) throw new ArgumentNullException(nameof(startPoint));
            if (direction == null) throw new ArgumentNullException(nameof(direction));
            if (function.OrderedArguments.Count != startPoint.Dimension)
                throw new ArgumentException($"Function has {function.OrderedArguments.Count} arguments, but start point has dimension of {startPoint.Dimension}");
            if (function.OrderedArguments.Count != direction.Dimension)
                throw new ArgumentException($"Function has {function.OrderedArguments.Count} arguments, but direction has dimension of {direction.Dimension}");
            
            var interval = _intervalSearchStrategy.SearchInterval(function, startPoint, direction);
            direction = interval.Direction;

            double a = 0;
            var b = interval.Length;

            var lambda = a + Tau2 * Math.Abs(b - a);
            var mu = a + Tau1 * Math.Abs(b - a);

            var limiter = new IterationLimiter(_settings);
            do
            {
                limiter.NextIteration();

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

                if (Math.Abs(b - a) <= _settings.Accuracy || limiter.ShouldInterrupt)
                    return new Interval(interval.First.Move(direction, a), interval.First.Move(direction, b));

            }
            while (true);            
        }
    }
}