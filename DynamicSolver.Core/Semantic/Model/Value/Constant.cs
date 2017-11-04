using System;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model.Value
{
    public class Constant : IValueSource
    {
        public double Value { get; }

        public Constant(double value)
        {
            if (double.IsNaN(value)) throw new ArgumentException("Value should represent an actual number.");
            if (double.IsInfinity(value)) throw new ArgumentException("Value should be finite.");

            Value = value;
        }
        
        public T Accept<T>([NotNull] ISemanticVisitor<T> visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            return visitor.Visit(this);
        }
    }
}