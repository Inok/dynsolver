using System;
using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Syntax.Model
{
    public class VariablePrimitive : IPrimitive, IEquatable<VariablePrimitive>
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

        public bool Equals(VariablePrimitive other)
        {
            if (other == null) return false;

            return Name.Equals(other.Name, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((VariablePrimitive) obj);
        }

        public bool Equals(ISyntaxExpression other)
        {
            return this.Equals((object)other);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}