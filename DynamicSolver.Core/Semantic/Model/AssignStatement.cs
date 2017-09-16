using System;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model
{
    public class AssignStatement : IStatement
    {
        [NotNull]
        public IValueSource Source { get; }

        [NotNull]
        public IValueTarget Target { get; }

        public AssignStatement([NotNull] IValueTarget target, [NotNull] IValueSource source)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }
        
        public T Accept<T>([NotNull] ISemanticVisitor<T> visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            return visitor.Visit(this);
        }
    }
}