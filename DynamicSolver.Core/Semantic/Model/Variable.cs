using System;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model
{
    public class Variable : IValueSource, IValueTarget
    {
        [CanBeNull]
        public string ExplicitName { get; }

        public Variable()
        {
            ExplicitName = null;
        }

        public Variable([NotNull] string explicitName)
        {
            ValidateName(explicitName);
            ExplicitName = explicitName;
        }

        public T Accept<T>([NotNull] ISemanticVisitor<T> visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            return visitor.Visit(this);
        }

        private static void ValidateName(string explicitName)
        {
            if (string.IsNullOrEmpty(explicitName))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(explicitName));
            }

            if (!char.IsLetter(explicitName[0]))
            {
                throw new ArgumentException(
                    $"First character of a variable name must be a unicode letter, but actual name is '{explicitName}'.",
                    nameof(explicitName));
            }

            for (var i = 0; i < explicitName.Length; i++)
            {
                var ch = explicitName[i];
                if (char.IsLetterOrDigit(ch) || char.IsNumber(ch))
                {
                    continue;
                }

                throw new ArgumentException(
                    $"Variable name must contain only unicode letters and numbers, but name '{explicitName}' contains invalid character '{ch}' at position '{i}'.",
                    nameof(explicitName));
            }
        }
    }
}