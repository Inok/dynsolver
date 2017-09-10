using System;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model
{
    public class FunctionCallOperation : IValueSource
    {
        public Function Function { get; }

        [NotNull]
        public IValueSource Argument { get; }

        public FunctionCallOperation(Function function, [NotNull] IValueSource argument)
        {
            if (argument == null) throw new ArgumentNullException(nameof(argument));
            if (!Enum.IsDefined(typeof(Function), function)) throw new ArgumentOutOfRangeException(nameof(function), "Value should be defined in the Function enum.");


            Function = function;
            Argument = argument;
        }
        
        public T Accept<T>([NotNull] ISemanticVisitor<T> visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            return visitor.Visit(this);
        }
    }
}