using System;
using System.Linq;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model.Struct
{
    public class StructElementAccessOperation : IValueSource, IValueTarget
    {
        [NotNull]
        public StructDeclaration StructDeclaration { get; }

        [NotNull]
        public StructElementDefinition Element { get; }

        public StructElementAccessOperation([NotNull] StructDeclaration structDeclaration, [NotNull] StructElementDefinition element)
        {
            if (structDeclaration == null) throw new ArgumentNullException(nameof(structDeclaration));
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (!structDeclaration.Definition.Elements.Contains(element))
            {
                throw new ArgumentException("Struct declaration doesn't contain provided element declaration.");
            }

            StructDeclaration = structDeclaration;
            Element = element;
        }


        public T Accept<T>(ISemanticVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}