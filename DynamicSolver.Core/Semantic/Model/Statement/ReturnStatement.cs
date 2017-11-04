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
            return visitor.Visit(this);
        }
    }
}