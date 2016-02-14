using System;
using DynamicSolver.Abstractions.Tools;
using JetBrains.Annotations;

namespace DynamicSolver.LinearAlgebra
{
    public class Point : ICloneable, IEquatable<Point>
    {
        [NotNull]
        private readonly double[] _point;

        public int Dimension => _point.Length;

        public double this[int i] => _point[i];

        public Point([NotNull] double[] point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            if (point.Length == 0) throw new ArgumentException("Argument is an empty collection", nameof(point));

            var copy = new double[point.Length];
            Array.Copy(point, copy, point.Length);
            _point = copy;
        }

        public object Clone()
        {
            var copy = new double[_point.Length];
            Array.Copy(_point, copy, _point.Length);
            return new Point(copy);
        }


        bool IEquatable<Point>.Equals([NotNull] Point other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if(Dimension != other.Dimension)
            {
                throw new ArgumentException($"Points has different dimensions: {Dimension} and {other.Dimension}");
            }

            for (var i = 0; i < _point.Length; i++)
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_point[i] != other[i])
                    return false;
            }
            return true;
        }

        public bool Equals(Point other, double epsilon)
        {
            if (epsilon < 0) throw new ArgumentOutOfRangeException(nameof(epsilon));

            if (other == null) throw new ArgumentNullException(nameof(other));
            if (Dimension != other.Dimension)
            {
                throw new ArgumentException($"Points has different dimensions: {Dimension} and {other.Dimension}");
            }

            var difference = 0D;
            for (var i = 0; i < _point.Length; i++)
            {
                difference += _point[i] - other[i];                    
            }
            difference = Math.Sqrt(difference);

            return Math.Abs(difference) < epsilon;
        }

        public override string ToString()
        {
            return _point.DumpInline();
        }
    }
}