using System;
using System.Globalization;
using DynamicSolver.Abstractions.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.ExpressionParser.Expression
{
    public class NumericPrimitive : IPrimitive
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
    }
}