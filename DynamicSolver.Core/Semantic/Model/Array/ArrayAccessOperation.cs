using System;
using DynamicSolver.Core.Semantic.Model.Type;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model.Array
{
    public class ArrayAccessOperation : IValueSource, IValueTarget
    {
        [NotNull]
        public ArrayDeclaration Array { get; }

        public int Index { get; }
        
        [NotNull]
        public IValueType ValueType { get; }

        public ArrayAccessOperation([NotNull] ArrayDeclaration array, int index)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (index < 0 || index >= array.Size) throw new ArgumentException($"Index '{index}' is out of boundaries of array with size '{array.Size}'.");

            Array = array;
            Index = index;
            ValueType = array.ValueType;
        }

        public T Accept<T>(ISemanticVisitor<T> visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            return visitor.Visit(this);
        }
    }
}