using System;
using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Semantic.Model
{
    public class AssignStatement : IStatement
    {
        [NotNull]
        public IValueSource Source { get; }

        [NotNull]
        public IValueTarget Target { get; }

        public AssignStatement([NotNull] IValueTarget target, [NotNull] IValueSource source)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (source == null) throw new ArgumentNullException(nameof(source));

            Target = target;
            Source = source;
        }
        
        public T Accept<T>([NotNull] ISemanticVisitor<T> visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            return visitor.Visit(this);
        }
    }
}