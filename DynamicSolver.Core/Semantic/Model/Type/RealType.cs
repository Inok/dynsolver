using System;
using System.ComponentModel;

namespace DynamicSolver.Core.Semantic.Model.Type
{
    public sealed class RealType : IValueType
    {
        public static RealType Single { get; } = new RealType(RealTypeKind.FloatingPointSingle);
        public static RealType Double { get; } = new RealType(RealTypeKind.FloatingPointDouble);

        public RealTypeKind Kind { get; }

        public double MinValue => GetMinValue(Kind);
        public double MaxValue => GetMaxValue(Kind);

        public RealType(RealTypeKind kind)
        {
            if (!Enum.IsDefined(typeof(RealTypeKind), kind))
                throw new InvalidEnumArgumentException(nameof(kind), (int) kind, typeof(RealTypeKind));
            Kind = kind;
        }

        public static double GetMinValue(RealTypeKind kind)
        {
            if (!Enum.IsDefined(typeof(RealTypeKind), kind))
                throw new InvalidEnumArgumentException(nameof(kind), (int) kind, typeof(RealTypeKind));

            switch (kind)
            {
                case RealTypeKind.FloatingPointSingle:
                    return float.MinValue;
                case RealTypeKind.FloatingPointDouble:
                    return double.MinValue;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }

        public static double GetMaxValue(RealTypeKind kind)
        {
            if (!Enum.IsDefined(typeof(RealTypeKind), kind))
                throw new InvalidEnumArgumentException(nameof(kind), (int) kind, typeof(RealTypeKind));
            
            switch (kind)
            {
                case RealTypeKind.FloatingPointSingle:
                    return float.MaxValue;
                case RealTypeKind.FloatingPointDouble:
                    return double.MaxValue;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }

        public override string ToString()
        {
            return Kind.ToString("G");
        }

        public bool Equals(IValueType other)
        {
            return Equals((object) other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is RealType realType && Kind == realType.Kind;
        }

        public override int GetHashCode()
        {
            return (int) Kind;
        }
    }
}