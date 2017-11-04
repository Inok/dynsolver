using System;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model.Struct
{
    public class StructDeclaration : IDeclaration
    {
        [NotNull]
        public StructDefinition Definition { get; }
        
        public ElementName ExplicitName { get; }

        public StructDeclaration([NotNull] StructDefinition definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        public StructDeclaration([NotNull] StructDefinition definition, [NotNull] ElementName explicitName)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            ExplicitName = explicitName ?? throw new ArgumentNullException(nameof(explicitName));
        }
    }
}