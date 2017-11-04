using System;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model.Operation
{
    public class MathFunctionCallOperation : IValueSource
    {
        public MathFunction MathFunction { get; }

        [NotNull]
        public IValueSource Argument { get; }

        public MathFunctionCallOperation(MathFunction mathFunction, [NotNull] IValueSource argument)
        {
            if (argument == null) throw new ArgumentNullException(nameof(argument));
            if (!Enum.IsDefined(typeof(MathFunction), mathFunction)) throw new ArgumentOutOfRangeException(nameof(mathFunction), "Value should be defined in the Function enum.");


            MathFunction = mathFunction;
            Argument = argument;
        }
        
        public T Accept<T>([NotNull] ISemanticVisitor<T> visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            return visitor.Visit(this);
        }
    }
}