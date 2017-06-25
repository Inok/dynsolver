using System;
using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Semantic.Model
{
    public class Constant : IValueSource
    {
        public double Value { get; }

        public Constant(double value)
        {
            Value = value;
        }
        
        public T Accept<T>([NotNull] ISemanticVisitor<T> visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            return visitor.Visit(this);
        }
    }
}