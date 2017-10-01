using System;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model
{
    public class ElementName : IEquatable<ElementName>
    {
        [NotNull]
        private string Name { get; }

        public ElementName([NotNull] string name)
        {
            ValidateName(name);
            Name = name;
        }

        public static void ValidateName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            }

            if (!char.IsLetter(name[0]))
            {
                throw new ArgumentException(
                    $"First character of a name must be a unicode letter, but actual name is '{name}'.",
                    nameof(name));
            }

            for (var i = 0; i < name.Length; i++)
            {
                var ch = name[i];
                if (char.IsLetterOrDigit(ch) || char.IsNumber(ch) || ch == '_')
                {
                    continue;
                }

                throw new ArgumentException(
                    $"Name must contain only unicode letters, numbers and underscores, but name '{name}' contains invalid character '{ch}' at position '{i}'.",
                    nameof(name));
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public static implicit operator string(ElementName self)
        {
            return self.Name;
        }

        public bool Equals(ElementName other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ElementName) obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static bool operator ==(ElementName left, ElementName right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ElementName left, ElementName right)
        {
            return !Equals(left, right);
        }
    }
}