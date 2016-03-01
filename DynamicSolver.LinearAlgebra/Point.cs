using System;
using System.Collections;
using System.Collections.Generic;
using DynamicSolver.Abstractions.Tools;
using JetBrains.Annotations;

namespace DynamicSolver.LinearAlgebra
{
    public class Point : ICloneable, IEquatable<Point>, IReadOnlyCollection<double>
    {
        [NotNull]
        private readonly double[] _point;

        public int Dimension => _point.Length;

        public double this[int i] => _point[i];

        public Point([NotNull] params double[] point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            if (point.Length == 0) throw new ArgumentException("Argument is an empty collection", nameof(point));

            var copy = new double[point.Length];
            Array.Copy(point, copy, point.Length);
            _point = copy;
        }

        public Point Move([NotNull] Vector direction, double distance)
        {
            if (direction == null) throw new ArgumentNullException(nameof(direction));
            if (direction.Dimension != Dimension) throw new ArgumentException("Point and direction has different dimensions.");

            if (direction.Length == 0)
            {
                return new Point(_point);
            }

            var movedPoint = new double[Dimension];
            for (var i = 0; i < movedPoint.Length; i++)
            {
                movedPoint[i] = _point[i] + direction[i] * distance;
            }
            return new Point(movedPoint);
        }

        public override string ToString()
        {
            return _point.DumpInline();
        }

        public object Clone()
        {
            var copy = new double[_point.Length];
            Array.Copy(_point, copy, _point.Length);
            return new Point(copy);
        }

        #region IEquatable<Point>

        public override int GetHashCode()
        {
            return _point.GetHashCode();
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.GetType() != this.GetType()) return false;
            return ((IEquatable<Point>)this).Equals((Point) other);
        }

        bool IEquatable<Point>.Equals(Point other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Dimension != other.Dimension) return false;

            for (var i = 0; i < _point.Length; i++)
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_point[i] != other[i])
                    return false;
            }
            return true;
        }

        public bool Equals(Point other, double accuracy)
        {
            if (accuracy < 0) throw new ArgumentOutOfRangeException(nameof(accuracy));

            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Dimension != other.Dimension) return false;

            var difference = 0D;
            for (var i = 0; i < _point.Length; i++)
            {
                var diff = _point[i] - other[i];
                difference += diff * diff;
            }
            difference = Math.Sqrt(difference);

            return difference <= accuracy;
        }

        public static bool operator ==(Point left, Point right)
        {
            return ReferenceEquals(null, left) ? ReferenceEquals(null, right) : left.Equals(right);
        }

        public static bool operator !=(Point left, Point right)
        {
            return !(left == right);
        }

        #endregion

        #region IReadOnlyCollection<double>

        public IEnumerator<double> GetEnumerator()
        {
            return new GenericEnumerator<double>(_point.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _point.GetEnumerator();
        }

        int IReadOnlyCollection<double>.Count => Dimension;

        #endregion
    }
}