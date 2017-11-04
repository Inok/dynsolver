using System;
using DynamicSolver.Core.Semantic.Model.Type;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model.Array
{
    public class ArrayDeclaration : IDeclaration
    {
        public ElementName ExplicitName { get; }
        
        [NotNull]
        public IValueType ValueType { get; }

        public int Size { get; }

        public ArrayDeclaration([NotNull] IValueType type, int size)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
            
            ValueType = type;
            Size = size;
        }

        public ArrayDeclaration([NotNull] IValueType type, [NotNull] string explicitName, int size) : this(type, size)
        {
            ExplicitName = new ElementName(explicitName);
        }
    }
}