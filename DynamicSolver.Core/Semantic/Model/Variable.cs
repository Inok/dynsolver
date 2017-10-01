using System;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model
{
    public class Variable : IValueSource, IValueTarget, IDeclaration
    {
        public ElementName ExplicitName { get; }

        public Variable()
        {
            ExplicitName = null;
        }

        public Variable([NotNull] string explicitName)
        {
            ExplicitName = new ElementName(explicitName);
        }

        public T Accept<T>([NotNull] ISemanticVisitor<T> visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            return visitor.Visit(this);
        }

    }
}