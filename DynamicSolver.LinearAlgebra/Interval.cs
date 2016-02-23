using System;
using JetBrains.Annotations;

namespace DynamicSolver.LinearAlgebra
{
    public class Interval
    {
        public Point First { get; }

        public Point Second { get; }

        private double? _length;
        public double Length => _length ?? (_length = new Vector(First, Second).Length).Value;

        public Interval([NotNull] Point first, [NotNull] Point second)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));
            if (first.Dimension != second.Dimension) throw new ArgumentException("Points has different dimensions.");
            
            First = first;
            Second = second;
        }
    }
}