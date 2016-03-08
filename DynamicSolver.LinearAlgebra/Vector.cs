using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace DynamicSolver.LinearAlgebra
{
    public class Vector : ICloneable, IReadOnlyCollection<double>
    {
        private readonly double[] _vector;

        public int Dimension => _vector.Length;

        public double Length => Math.Sqrt(_vector.Sum(d => d*d));

        public double this[int i] => _vector[i];

        public Vector([NotNull] params double[] vector)
        {
            if (vector == null) throw new ArgumentNullException(nameof(vector));
            if (vector.Length == 0) throw new ArgumentException("Argument is an empty collection", nameof(vector));

            _vector = new double[vector.Length];
            Array.Copy(vector, _vector, vector.Length);            
        }

        public Vector([NotNull] Point to)
        {
            if (to == null) throw new ArgumentNullException(nameof(to));

            _vector = new double[to.Dimension];
            for (var i = 0; i < _vector.Length; i++)
                _vector[i] = to[i];
        }

        public Vector([NotNull] Point from, [NotNull] Point to)
        {
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (to == null) throw new ArgumentNullException(nameof(to));
            if (from.Dimension != to.Dimension || from.Dimension == 0) throw new ArgumentException("Points has different dimensions.");
            
            _vector = new double[from.Dimension];
            for (var i = 0; i < _vector.Length; i++)
                _vector[i] = to[i] - from[i];
        }

        public Vector Normalize()
        {
            var length = Length;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (length == 0)
            {
                throw new InvalidOperationException("Cannot normalize zero-length vector."); 
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (length == 1)
            {
                return Clone();
            }

            return this*(1.0/length);
        }

        public override string ToString()
        {
            return $"({string.Join(",", _vector)})";
        }

        public static Vector operator +(Vector left, Vector right)
        {
            if (left.Dimension != right.Dimension)
                throw new ArgumentException($"Dimension mismatch: {left.Dimension} != {right.Dimension}");
            var res = new double[left.Dimension];
            for (var i = 0; i < res.Length; i++)
                res[i] = left[i] + right[i];
            return new Vector(res);
        }

        public static Vector operator -(Vector left, Vector right)
        {
            if (left.Dimension != right.Dimension)
                throw new ArgumentException($"Dimension mismatch: {left.Dimension} != {right.Dimension}");
            return left + (-right);
        }

        public static Vector operator -(Vector vec)
        {
            var res = new double[vec.Dimension];
            for (var i = 0; i < res.Length; i++)
                res[i] = -vec[i];
            return new Vector(res);
        }

        public static double operator *(Vector left, Vector right)
        {
            if (left.Dimension != right.Dimension)
                throw new ArgumentException($"Dimension mismatch: {left.Dimension} != {right.Dimension}");
            double result = 0;
            for (var i = 0; i < left.Dimension; i++)
                result += left[i]*right[i];
            return result;
        }

        public static Vector operator *(Vector left, double right)
        {
            var result = new double[left.Dimension];
            for (var i = 0; i < left.Dimension; i++)
                result[i] = left[i]*right;
            return new Vector(result);
        }

        public static Vector operator *(double left, Vector right)
        {
            return right*left;
        }

        #region ICloneable

        object ICloneable.Clone()
        {
            return Clone();
        }

        public Vector Clone()
        {
            return new Vector(_vector);
        }

        #endregion

        #region IReadOnlyCollection<double>

        public IEnumerator<double> GetEnumerator()
        {
            return new GenericEnumerator<double>(_vector.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _vector.GetEnumerator();
        }

        int IReadOnlyCollection<double>.Count => Dimension;

        #endregion


        public static Vector GetCoordinateDirection(int dimension, int coordinate)
        {
            if (dimension <= 0) throw new ArgumentOutOfRangeException(nameof(dimension));
            if (coordinate < 0 || coordinate >= dimension) throw new ArgumentOutOfRangeException(nameof(coordinate));

            var vector = new double[dimension];
            vector[coordinate] = 1;
            return new Vector(vector);
        }
    }
}