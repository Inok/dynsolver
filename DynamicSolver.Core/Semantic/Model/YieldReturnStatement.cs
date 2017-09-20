using System;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model
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
            return visitor.Visit(this);
        }
    }
}