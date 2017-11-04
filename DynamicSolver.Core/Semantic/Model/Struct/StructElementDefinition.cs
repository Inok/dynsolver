using System;
using DynamicSolver.Core.Semantic.Model.Type;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model.Struct
{
    public class StructElementDefinition
    {
        [NotNull]
        public IValueType ValueType { get; }
        
        [CanBeNull]
        public ElementName ExplicitName { get; }

        public StructElementDefinition([NotNull] IValueType type)
        {
            ValueType = type ?? throw new ArgumentNullException(nameof(type));
        }

        public StructElementDefinition(IValueType type, [NotNull] string explicitName) : this(type)
        {
            ExplicitName = new ElementName(explicitName);
        }
    }
}