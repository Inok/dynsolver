using System;
using System.Linq;
using JetBrains.Annotations;

namespace DynamicSolver.ExpressionParser.Expression
{
    public class Constant
    {
        public static Constant Pi { get; } = new Constant("pi", Math.PI, new[] {"pi", "PI"});
        public static Constant E { get; } = new Constant("e", Math.E, new[] {"e", "E"});

        private static readonly Constant[] Constants = new[] { Pi, E };

        public static Constant TryParse(string name)
        {
            return Constants.FirstOrDefault(constant => constant._parsedNames.Contains(name));
        }

        public static Constant Parse([NotNull] string name)
        {
            var constant = TryParse(name);
            if (constant != null)
                return constant;

            throw new FormatException("Provided name cannot be parsed as constant.");
        }

        private readonly string[] _parsedNames;

        public double Value { get; }
        public string Name { get; }

        private Constant(string name, double value, string[] parsedNames)
        {
            _parsedNames = parsedNames;
            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}