using System;
using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Semantic.Model
{
    public class AddOperation : IValueSource
    {
        [NotNull]
        public IValueSource Left { get; }

        [NotNull]
        public IValueSource Right { get; }

        public AddOperation([NotNull] IValueSource left, [NotNull] IValueSource right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            Left = left;
            Right = right;
        }

        public T Accept<T>([NotNull] ISemanticVisitor<T> visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            return visitor.Visit(this);
        }
    }
}