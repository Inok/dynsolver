using System;
using DynamicSolver.Abstractions.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.ExpressionParser.Expression
{
    public class VariablePrimitive : IPrimitive
    {
        [NotNull]
        public string Name { get; }

        public VariablePrimitive([NotNull] string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Argument is null or empty", nameof(name));
            Name = name;
        }

        public override string ToString()
        {
            return $"{Name}";
        }

    }
}