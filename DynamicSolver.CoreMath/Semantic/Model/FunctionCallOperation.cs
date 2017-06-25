using System;
using System.ComponentModel;
using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Semantic.Model
{
    public class FunctionCallOperation : IValueSource
    {
        public Function Function { get; }

        [NotNull]
        public IValueSource Argument { get; }

        public FunctionCallOperation(Function function, [NotNull] IValueSource argument)
        {
            if (argument == null) throw new ArgumentNullException(nameof(argument));
            if (!Enum.IsDefined(typeof(Function), function))
                throw new InvalidEnumArgumentException(nameof(function), (int) function, typeof(Function));

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