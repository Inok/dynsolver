using System;
using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Semantic.Model
{
    public class MinusOperation : IValueSource
    {
        [NotNull]
        public IValueSource Operand { get; }

        public MinusOperation([NotNull] IValueSource operand)
        {
            if (operand == null) throw new ArgumentNullException(nameof(operand));

            Operand = operand;
        }
        
        public T Accept<T>([NotNull] ISemanticVisitor<T> visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            return visitor.Visit(this);
        }
    }
}