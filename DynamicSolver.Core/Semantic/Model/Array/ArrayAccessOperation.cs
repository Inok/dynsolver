using System;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model.Array
{
    public class ArrayAccessOperation : IValueSource, IValueTarget
    {
        [NotNull]
        public ArrayDeclaration Array { get; }

        public int Index { get; }

        public ArrayAccessOperation([NotNull] ArrayDeclaration array, int index)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (index < 0 || index >= array.Size) throw new ArgumentException($"Index '{index}' is out of boundaries of array with size '{array.Size}'.");

            Array = array;
            Index = index;
        }

        public T Accept<T>(ISemanticVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}