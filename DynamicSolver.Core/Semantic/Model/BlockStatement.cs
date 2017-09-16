using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model
{
    public class BlockStatement : IStatement
    {
        [NotNull, ItemNotNull]
        public IReadOnlyCollection<IStatement> Statements { get; }

        public BlockStatement([NotNull, ItemNotNull] IEnumerable<IStatement> statements) : this(statements?.ToArray())
        {
            
        }
        
        public BlockStatement([NotNull, ItemNotNull]  IReadOnlyCollection<IStatement> statements)
        {
            if (statements == null) throw new ArgumentNullException(nameof(statements));
            if (statements.Count == 0) throw new ArgumentException("Statement collection is empty.", nameof(statements));
            if (statements.Any(s => s == null)) throw new ArgumentException("Statements collection contains null statements.", nameof(statements));

            Statements = statements;
        }

        public T Accept<T>(ISemanticVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}