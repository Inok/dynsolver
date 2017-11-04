using System;
using DynamicSolver.Core.Semantic.Model.Type;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model.Operation
{
    public class AddOperation : IValueSource
    {
        [NotNull]
        public IValueType ValueType { get; }

        [NotNull]
        public IValueSource Left { get; }

        [NotNull]
        public IValueSource Right { get; }

        public AddOperation([NotNull] IValueSource left, [NotNull] IValueSource right)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
            ValueType = MutualValueTypeResolver.GetMutualType(left.ValueType, right.ValueType);
        }

        public T Accept<T>(ISemanticVisitor<T> visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            return visitor.Visit(this);
        }
    }
}