using System;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model.Statement
{
    public class RepeatStatement : IStatement
    {
        public int RepeatsCount { get; }

        [NotNull]
        public IStatement Body { get; }

        public RepeatStatement(int repeatsCount, [NotNull] IStatement body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            if (repeatsCount <= 0) throw new ArgumentOutOfRangeException(nameof(repeatsCount));

            RepeatsCount = repeatsCount;
            Body = body;
        }

        public T Accept<T>(ISemanticVisitor<T> visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            return visitor.Visit(this);
        }
    }
}