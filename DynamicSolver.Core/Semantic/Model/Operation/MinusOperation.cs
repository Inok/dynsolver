using System;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model.Operation
{
    public class MinusOperation : IValueSource
    {
        [NotNull]
        public IValueSource Operand { get; }

        public MinusOperation([NotNull] IValueSource operand)
        {
            Operand = operand ?? throw new ArgumentNullException(nameof(operand));
        }
        
        public T Accept<T>([NotNull] ISemanticVisitor<T> visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            return visitor.Visit(this);
        }
    }
}