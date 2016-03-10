using System;
using System.Linq;
using DynamicSolver.Abstractions;
using DynamicSolver.LinearAlgebra;
using JetBrains.Annotations;

namespace DynamicSolver.Minimizer.MultiDimensionalSearch
{
    public class RosenbrockMethod : IMultiDimensionalSearchStrategy
    {
        private readonly IDirectedSearchStrategy _directedSearchStrategy;
        private readonly MultiDimensionalSearchSettings _settings;

        public RosenbrockMethod([NotNull] IDirectedSearchStrategy directedSearchStrategy, [NotNull] MultiDimensionalSearchSettings settings)
        {
            if (directedSearchStrategy == null) throw new ArgumentNullException(nameof(directedSearchStrategy));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            _directedSearchStrategy = directedSearchStrategy;
            _settings = settings;
        }

        public Point Search(IExecutableFunction function, Point startPoint)
        {
            var x1 = startPoint;
            var limiter = new IterationLimiter(_settings);

            var directions = new Vector[startPoint.Dimension];

            for (var i = 0; i < directions.Length; i++)
            {
                directions[i] = Vector.GetCoordinateDirection(startPoint.Dimension, i);
            }
            
            var shifts = new double[startPoint.Dimension];

            do
            {
                limiter.NextIteration();

                var x2 = x1;
                for (var i = 0; i < directions.Length; i++)
                {
                    var tmp = _directedSearchStrategy.SearchInterval(function, x2, directions[i]).Center;
                    if (function.Execute(tmp.ToArray()) < function.Execute(x2.ToArray()))
                    {
                        shifts[i] = new Interval(x2, tmp).Length;
                        x2 = tmp;
                    }
                    else
                    {
                        shifts[i] = 0;
                    }
                }

                if (new Interval(x1, x2).Length < _settings.Accuracy || limiter.ShouldInterrupt)
                {
                    return x2;
                }

                x1 = x2;

                var a = new Vector[startPoint.Dimension];
                for (var i = 0; i < a.Length; i++)
                {
                    if (shifts[i] == 0)
                    {
                        a[i] = directions[i];
                    }
                    else
                    {
                        a[i] = new Vector(new double[startPoint.Dimension]);
                        for (int j = i; j < startPoint.Dimension; j++)
                        {
                            a[i] = a[i] + shifts[j]*directions[j];
                        }                        
                    }
                }

                var b = new Vector[startPoint.Dimension];
                for (var i = 0; i < b.Length; i++)
                {
                    b[i] = a[i];

                    for (int j = 0; j < i; j++)
                    {
                        b[i] = b[i] - (a[i]*directions[j]) * directions[j];
                    }
                }

                for (var i = 0; i < b.Length; i++)
                {
                    directions[i] = b[i].Normalize();                    
                }                
            }
            while (true);
        }
    }
}