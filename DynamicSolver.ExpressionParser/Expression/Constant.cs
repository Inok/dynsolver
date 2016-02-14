using System;
using System.Linq;
using JetBrains.Annotations;

namespace DynamicSolver.ExpressionParser.Expression
{
    public class Constant
    {
        [NotNull]
        public static Constant Pi { get; } = new Constant("pi", Math.PI, new[] {"pi", "PI"});

        [NotNull]
        public static Constant E { get; } = new Constant("e", Math.E, new[] {"e", "E"});

        [NotNull, ItemNotNull]
        private static readonly Constant[] Constants = { Pi, E };

        [CanBeNull]
        public static Constant TryParse([NotNull] string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            return Constants.FirstOrDefault(c => c._parsedNames.Contains(name));
        }

        [NotNull]
        public static Constant Parse([NotNull] string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            var constant = TryParse(name);
            if (constant != null)
                return constant;

            throw new FormatException("Provided name cannot be parsed as constant.");
        }

        [NotNull,ItemNotNull]
        private readonly string[] _parsedNames;

        [NotNull]
        public string Name { get; }

        public double Value { get; }

        private Constant([NotNull] string name, double value, [NotNull] string[] parsedNames)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (parsedNames == null) throw new ArgumentNullException(nameof(parsedNames));
            if (parsedNames.Length == 0) throw new ArgumentException("parsedNames is an empty collection", nameof(parsedNames));
            if (parsedNames.Any(n => n == null)) throw new ArgumentException("parsedNames contains null value.", nameof(parsedNames));

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