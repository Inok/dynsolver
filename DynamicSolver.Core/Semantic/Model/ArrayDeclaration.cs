using System;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model
{
    public class ArrayDeclaration : IDeclaration
    {
        public ElementName ExplicitName { get; }
        
        public int Size { get; }

        public ArrayDeclaration(int size)
        {
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
            Size = size;
        }

        public ArrayDeclaration([NotNull] string explicitName, int size) : this(size)
        {
            ExplicitName = new ElementName(explicitName);
        }
    }
}