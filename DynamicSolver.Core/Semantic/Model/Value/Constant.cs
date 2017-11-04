using System;
using DynamicSolver.Core.Semantic.Model.Type;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model.Value
{
    public abstract class Constant : IConstantValue
    {
        [NotNull]
        public IValueType ValueType { get; }

        protected Constant([NotNull] IValueType valueType)
        {
            ValueType = valueType ?? throw new ArgumentNullException(nameof(valueType));
        }

        public abstract T Accept<T>(ISemanticVisitor<T> visitor);
    }
}