using System;
using DynamicSolver.Core.Semantic.Model.Type;

namespace DynamicSolver.Core.Semantic.Model.Value
{
    public class RealConstant : Constant
    {
        public double Value { get; }

        public RealConstant(float value) : base(RealType.Single)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(nameof(value), $"Value '{value}' is NaN or infinity.");
            
            Value = value;
        }
        
        public RealConstant(double value) : base(RealType.Double)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentOutOfRangeException(nameof(value), $"Value '{value}' is NaN or infinity.");
            
            Value = value;
        }
        
        public RealConstant(RealType type, double value) : base(type)
        {
            if (double.IsNaN(value)) throw new ArgumentOutOfRangeException(nameof(value), $"Value '{value}' is NaN or infinity.");
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