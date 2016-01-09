﻿using System;
using System.Globalization;
using DynamicSolver.Abstractions.Expression;

namespace DynamicSolver.ExpressionParser.Expression
{
    public class NumericPrimitive : IPrimitive
    {
        private static readonly IFormatProvider DoubleFormat = new NumberFormatInfo() { NumberDecimalSeparator = "." };

        public string Token { get; }
        public double Value { get; }

        public NumericPrimitive(string token)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentException("Argument is null or empty", nameof(token));

            Token = token;
            
            Value = double.Parse(token, DoubleFormat);
        }

        public override string ToString()
        {
            return $"{Token}";
        }
    }
}