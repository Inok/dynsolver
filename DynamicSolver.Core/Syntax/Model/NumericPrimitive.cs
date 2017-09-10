using System;
using System.Globalization;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Syntax.Model
{
    public class NumericPrimitive : IPrimitive, IEquatable<NumericPrimitive>
    {
        private static readonly IFormatProvider DoubleFormat = new NumberFormatInfo() { NumberDecimalSeparator = "." };

        [NotNull]
        public string Token { get; }
        public double Value { get; }

        public NumericPrimitive([NotNull] string token)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentException("Argument is null or empty", nameof(token));

            Token = token;
            
            Value = double.Parse(token, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, DoubleFormat);
        }

        public override string ToString()
        {
            return $"{Token}";
        }

        public bool Equals(NumericPrimitive other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NumericPrimitive) obj);
        }

        public bool Equals(ISyntaxExpression other)
        {
            return this.Equals((object)other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}