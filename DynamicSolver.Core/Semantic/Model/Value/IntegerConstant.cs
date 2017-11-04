using System;
using System.Numerics;
using DynamicSolver.Core.Semantic.Model.Type;

namespace DynamicSolver.Core.Semantic.Model.Value
{
    public class IntegerConstant : Constant
    {
        public BigInteger Value { get; }

        public IntegerConstant(IntegerType type, int value) : this(type, new BigInteger(value))
        {
        }

        public IntegerConstant(IntegerType type, long value) : this(type, new BigInteger(value))
        {
        }

        public IntegerConstant(IntegerType type, BigInteger value) : base(type)
        {
            if (value < type.MinValue) throw new ArgumentOutOfRangeException($"Value '{value}' is less than min value '{type.MinValue}' allowed by type '{type}'");
            if (value > type.MaxValue) throw new ArgumentOutOfRangeException($"Value '{value}' is greater than max value '{type.MaxValue}' allowed by type '{type}'");
            
            Value = value;
        }
        
        public override T Accept<T>(ISemanticVisitor<T> visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            return visitor.Visit(this);
        }
    }
}