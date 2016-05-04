using System;
using System.Linq;
using System.Threading;
using DynamicSolver.Abstractions;
using DynamicSolver.LinearAlgebra;
using JetBrains.Annotations;

namespace DynamicSolver.Minimizer.MultiDimensionalSearch
{
    public class HookeJeevesMethod : IMultiDimensionalSearchStrategy
    {
        private readonly HookeJeevesSearchSettings _settings;

        public HookeJeevesMethod([NotNull] HookeJeevesSearchSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            _settings = settings;
        }

        public Point Search(IExecutableFunction function, Point startPoint, CancellationToken token = default(CancellationToken))
        {
            var x1 = startPoint;
            var increment = _settings.InitialIncrement;

            var limiter = new IterationLimiter(_settings);
            do
            {
                token.ThrowIfCancellationRequested();
                limiter.NextIteration();

                var internalLimiter = new IterationLimiter(_settings);
                Point x2;
                do
                {
                    token.ThrowIfCancellationRequested();
                    internalLimiter.NextIteration();

                    x2 = GetGreaterByCoordinateDirections(function, x1, increment);

                    if (x2 != x1 && function.Execute(x2.ToArray()) < function.Execute(x1.ToArray()))
                    {
                        break;
                    }

                    increment *= _settings.StepReductionFactor;

                    if (increment < _settings.Accuracy)
                    {
                        return x1;
                    }
                    
                } while (true);


                internalLimiter = new IterationLimiter(_settings);
                var x3 = x1;
                var x4 = x2;
                do
                {
                    token.ThrowIfCancellationRequested();
                    internalLimiter.NextIteration();

                    var tmp = GetGreaterByCoordinateDirections(function, x4.Move(new Vector(x3, x4)), increment);

                    if (function.Execute(tmp.ToArray()) >= function.Execute(x4.ToArray()))
                    {
                        break;
                    }

                    x3 = x4;
                    x4 = tmp;
                } while (true);

                var iterationChangeInterval = new Interval(x1, x4);
                if (iterationChangeInterval.Length < _settings.Accuracy || limiter.ShouldInterrupt)
                {
                    return x4;
                }

                increment *= _settings.StepReductionFactor;

                if (increment < _settings.Accuracy)
                {
                    return x4;
                }

                x1 = x4;
            }
            while (true);
        }

        [NotNull]
        private static Point GetGreaterByCoordinateDirections(IExecutableFunction function, Point current, double increment)
        {
            for (int i = 0; i < current.Dimension; i++)
            {
                var originalValue = function.Execute(current.ToArray());

                var positiveShiftedPoint = current.Move(Vector.GetCoordinateDirection(current.Dimension, i), increment);
                var negativeShiftedPoint = current.Move(Vector.GetCoordinateDirection(current.Dimension, i), -increment);

                var positiveShiftedValue = function.Execute(positiveShiftedPoint.ToArray());
                var negativeShiftedValue = function.Execute(negativeShiftedPoint.ToArray());

                if (positiveShiftedValue < originalValue)
                {
                    current = positiveShiftedPoint;
                    originalValue = positiveShiftedValue;
                }

                if (negativeShiftedValue < originalValue)
                {
                    current = negativeShiftedPoint;
                }
            }
            return current;
        }
    }
}