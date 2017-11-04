using System;
using DynamicSolver.Core.Semantic.Model.Type;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model.Operation
{
    public class MathFunctionCallOperation : IValueSource
    {
        public MathFunction MathFunction { get; }

        [NotNull]
        public IValueType ValueType { get; }

        [NotNull]
        public IValueSource Argument { get; }

        public MathFunctionCallOperation(MathFunction mathFunction, [NotNull] IValueSource argument)
        {
            if (argument == null) throw new ArgumentNullException(nameof(argument));
            if (!Enum.IsDefined(typeof(MathFunction), mathFunction)) throw new ArgumentOutOfRangeException(nameof(mathFunction), "Value should be defined in the Function enum.");

            MathFunction = mathFunction;
            Argument = argument;
            ValueType = argument.ValueType;
        }

        public T Accept<T>(ISemanticVisitor<T> visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            return visitor.Visit(this);
        }
    }
}