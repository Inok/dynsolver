using System;
using DynamicSolver.Core.Semantic.Model.Type;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model.Operation
{
    public class PowOperation : IValueSource
    {
        [NotNull]
        public IValueType ValueType { get; }

        [NotNull]
        public IValueSource Value { get; }

        [NotNull]
        public IValueSource Power { get; }

        public PowOperation([NotNull] IValueSource value, [NotNull] IValueSource power)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Power = power ?? throw new ArgumentNullException(nameof(power));
            ValueType = MutualValueTypeResolver.GetMutualType(value.ValueType, power.ValueType);
        }

        public T Accept<T>(ISemanticVisitor<T> visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            return visitor.Visit(this);
        }
    }
}