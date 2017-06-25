using System;
using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Semantic.Model
{
    public class Variable : IValueSource, IValueTarget
    {
        [CanBeNull]
        public string ExplicitName { get; }

        public Variable()
        {
            ExplicitName = null;
        }

        public Variable([NotNull] string explicitName)
        {
            if (string.IsNullOrEmpty(explicitName)) throw new ArgumentException("Value cannot be null or empty.", nameof(explicitName));

            ExplicitName = explicitName;
        }

        public T Accept<T>([NotNull] ISemanticVisitor<T> visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            return visitor.Visit(this);
        }
    }
}