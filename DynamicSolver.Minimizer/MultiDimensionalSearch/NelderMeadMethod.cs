using System;
using System.Linq;
using System.Threading;
using DynamicSolver.Abstractions;
using DynamicSolver.LinearAlgebra;
using JetBrains.Annotations;

namespace DynamicSolver.Minimizer.MultiDimensionalSearch
{
    public class NelderMeadMethod : IMultiDimensionalSearchStrategy
    {
        private class PointValuePair : IComparable<PointValuePair>
        {
            public Point Point { get; }
            public double Value { get; }

            public PointValuePair([NotNull] Point point, double value)
            {
                if (point == null) throw new ArgumentNullException(nameof(point));
                Point = point;
                Value = value;
            }

            public static PointValuePair Create(IExecutableFunction function, Vector point)
            {
                var args = point.ToArray();
                return new PointValuePair(new Point(args), function.Execute(args));
            }


            public int CompareTo(PointValuePair other)
            {
                return Value.CompareTo(other.Value);
            }
        }

        private readonly NelderMeadSearchSettings _settings;

        public NelderMeadMethod([NotNull] NelderMeadSearchSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            _settings = settings;
        }

        public Point Search(IExecutableFunction function, Point startPoint, CancellationToken token = default(CancellationToken))
        {
            if(startPoint.Dimension < 2) throw new ArgumentException("This method supports only dimensions greated then 1.");

            var values = CreateInitialSimplex(startPoint).Select(s => new PointValuePair(s, function.Execute(s.ToArray()))).ToArray();

            var limiter = new IterationLimiter(_settings);
            do
            {
                token.ThrowIfCancellationRequested();
                limiter.NextIteration();

                var ph = values.Max();
                var pg = values.Where(p => p != ph).Max();
                var pl = values.Min();


                var x0 = (values.Where(p => p != ph).Select(p => new Vector(p.Point)).Aggregate((a, v) => a + v) * (1.0/startPoint.Dimension));
                
                var xr = (1 + _settings.ReflectionCoefficient)*x0 - _settings.ReflectionCoefficient*new Vector(ph.Point);
                var pr = PointValuePair.Create(function, xr);

                if (pr.Value < pl.Value)
                {
                    var xe = _settings.ExpansionCoefficient * new Vector(pr.Point) + (1 - _settings.ExpansionCoefficient) * x0;
                    var pe = PointValuePair.Create(function, xe);

                    if (pe.Value < pl.Value)
                    {
                        values[Array.IndexOf(values, ph)] = pe;
                        if (CheckConvergence(values)) return pe.Point;
                        continue;
                    }

                    if (pe.Value > pl.Value)
                    {
                        Exchange(values, ph, pr);
                        if (CheckConvergence(values)) return pr.Point;
                        continue;
                    }
                }
                else if (pl.Value < pr.Value && pr.Value < pg.Value)
                {
                    Exchange(values, ph, pr);
                    if (CheckConvergence(values)) return pr.Point;
                    continue;
                }
                else if (pg.Value < pr.Value && pr.Value < ph.Value)
                {
                    Exchange(values, ph, pr);
                    var tmp = pr;
                    pr = ph;
                    ph = tmp;
                }

                var xs = _settings.CompressionCoefficient * new Vector(ph.Point) + (1 - _settings.CompressionCoefficient) * x0;
                var ps = PointValuePair.Create(function, xs);

                if (ps.Value < ph.Value)
                {
                    Exchange(values, ph, ps);
                    if (CheckConvergence(values)) return ps.Point;
                    continue;
                }

                if (ps.Value > ph.Value)
                {
                    var vmin = new Vector(pl.Point);
                    foreach (var val in values.Where(v => v != pl))
                    {
                        Exchange(values, val, PointValuePair.Create(function, vmin + (new Vector(val.Point) - vmin) * 0.5));
                    }
                }
                
                if (CheckConvergence(values)) return values.Min().Point;
            }
            while (true);
        }

        private bool CheckConvergence(PointValuePair[] values)
        {
            var f = values.Sum(v => v.Value)/values.Length;
            var dispersion = Math.Sqrt(values.Select(v => Math.Pow(v.Value - f, 2)).Sum() / values.Length);
            return dispersion <= _settings.Accuracy;
        }

        private static void Exchange(PointValuePair[] values, PointValuePair from, PointValuePair to)
        {
            values[Array.IndexOf(values, @from)] = to;
        }

        private static Point[] CreateInitialSimplex(Point startPoint)
        {
            var simplex = new Point[startPoint.Dimension + 1];
            simplex[0] = startPoint;
            for (var i = 1; i < simplex.Length; i++)
            {
                simplex[i] = startPoint.Move(Vector.GetCoordinateDirection(startPoint.Dimension, i - 1), 1);
            }
            return simplex;
        }
    }
}