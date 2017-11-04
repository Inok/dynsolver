using System;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model.Statement
{
    public class ReturnStatement : IStatement
    {
        [NotNull]
        public IValueSource Source { get; }
        
        public ReturnStatement([NotNull] IValueSource source)
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