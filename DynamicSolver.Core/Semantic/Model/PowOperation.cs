using System;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model
{
    public class PowOperation : IValueSource
    {
        [NotNull]
        public IValueSource Value { get; }

        [NotNull]
        public IValueSource Power { get; }

        public PowOperation([NotNull] IValueSource value, [NotNull] IValueSource power)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Power = power ?? throw new ArgumentNullException(nameof(power));
        }
        
        public T Accept<T>([NotNull] ISemanticVisitor<T> visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            return visitor.Visit(this);
        }
    }
}