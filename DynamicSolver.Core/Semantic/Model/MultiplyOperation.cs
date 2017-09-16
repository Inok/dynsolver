using System;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model
{
    public class MultiplyOperation : IValueSource
    {
        [NotNull]
        public IValueSource Left { get; }

        [NotNull]
        public IValueSource Right { get; }

        public MultiplyOperation([NotNull] IValueSource left, [NotNull] IValueSource right)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
        }
        
        public T Accept<T>([NotNull] ISemanticVisitor<T> visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            return visitor.Visit(this);
        }
    }
}