using System;
using DynamicSolver.Abstractions.Expression;

namespace DynamicSolver.ExpressionParser.Expression
{
    public class VariablePrimitive : IPrimitive
    {
        public string Name { get; }

        public VariablePrimitive(string name)
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