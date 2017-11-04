using System;
using DynamicSolver.Core.Semantic.Model.Type;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model.Operation
{
    public class MinusOperation : IValueSource
    {
        [NotNull]
        public IValueType ValueType { get; }

        [NotNull]
        public IValueSource Operand { get; }

        public MinusOperation([NotNull] IValueSource operand)
        {
            Operand = operand ?? throw new ArgumentNullException(nameof(operand));
            ValueType = operand.ValueType;
        }

        public T Accept<T>(ISemanticVisitor<T> visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            return visitor.Visit(this);
        }
    }
}