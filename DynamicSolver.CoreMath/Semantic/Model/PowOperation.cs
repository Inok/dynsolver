using System;
using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Semantic.Model
{
    public class PowOperation : IValueSource
    {
        [NotNull]
        public IValueSource Value { get; }

        [NotNull]
        public IValueSource Power { get; }

        public PowOperation([NotNull] IValueSource value, [NotNull] IValueSource power)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (power == null) throw new ArgumentNullException(nameof(power));

            Value = value;
            Power = power;
        }
        
        public T Accept<T>([NotNull] ISemanticVisitor<T> visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            return visitor.Visit(this);
        }
    }
}