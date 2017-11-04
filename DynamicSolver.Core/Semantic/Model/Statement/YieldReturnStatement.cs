using System;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model.Statement
{
    public class YieldReturnStatement : IStatement
    {
        [NotNull]
        public IValueSource Source { get; }

        public YieldReturnStatement([NotNull] IValueSource source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public T Accept<T>(ISemanticVisitor<T> visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            return visitor.Visit(this);
        }
    }
}