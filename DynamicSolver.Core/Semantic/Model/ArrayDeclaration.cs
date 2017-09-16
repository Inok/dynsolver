using System;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model
{
    public class ArrayDeclaration : IDeclaration
    {
        [CanBeNull]
        public string ExplicitName { get; }
        
        public int Size { get; }

        public ArrayDeclaration(int size)
        {
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
            Size = size;
        }

        public ArrayDeclaration([NotNull] string explicitName, int size) : this(size)
        {
            ValidateName(explicitName);
            ExplicitName = explicitName;
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
                    $"First character of an array name must be a unicode letter, but actual name is '{explicitName}'.",
                    nameof(explicitName));
            }

            for (var i = 0; i < explicitName.Length; i++)
            {
                var ch = explicitName[i];
                if (char.IsLetterOrDigit(ch) || char.IsNumber(ch) || ch == '_')
                {
                    continue;
                }

                throw new ArgumentException(
                    $"Array name must contain only unicode letters, numbers and underscores, but name '{explicitName}' contains invalid character '{ch}' at position '{i}'.",
                    nameof(explicitName));
            }
        }
    }
}