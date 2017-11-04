using System;
using DynamicSolver.Core.Semantic.Model.Type;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model.Value
{
    public class Variable : IValueSource, IValueTarget, IDeclaration
    {
        [NotNull]
        public IValueType ValueType { get; }
        
        public ElementName ExplicitName { get; }

        public Variable([NotNull] IValueType type)
        {
            ValueType = type ?? throw new ArgumentNullException(nameof(type));
            ExplicitName = null;
        }

        public Variable([NotNull] IValueType type, [NotNull] string explicitName) : this(type)
        {
            ExplicitName = new ElementName(explicitName);
        }

        public T Accept<T>(ISemanticVisitor<T> visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            return visitor.Visit(this);
        }
    }
}