using System;
using System.ComponentModel;
using System.Numerics;

namespace DynamicSolver.Core.Semantic.Model.Type
{
    public sealed class IntegerType : IValueType
    {
        public static IntegerType Int8 { get; } = new IntegerType(IntegerValueSize.Int8);
        public static IntegerType Int16 { get; } = new IntegerType(IntegerValueSize.Int16);
        public static IntegerType Int32 { get; } = new IntegerType(IntegerValueSize.Int32);
        public static IntegerType Int64 { get; } = new IntegerType(IntegerValueSize.Int64);

        public IntegerValueSize Size { get; }

        public BigInteger MinValue => GetMinValue(Size);
        public BigInteger MaxValue => GetMaxValue(Size);

        public IntegerType(IntegerValueSize size)
        {
            if (!Enum.IsDefined(typeof(IntegerValueSize), size))
                throw new InvalidEnumArgumentException(nameof(size), (int) size, typeof(IntegerValueSize));
            Size = size;
        }
        
        public static BigInteger GetMinValue(IntegerValueSize size)
        {
            if (!Enum.IsDefined(typeof(IntegerValueSize), size))
                throw new InvalidEnumArgumentException(nameof(size), (int) size, typeof(IntegerValueSize));
            
            switch (size)
            {
                case IntegerValueSize.Int8:
                    return new BigInteger(sbyte.MinValue);
                case IntegerValueSize.Int16:
                    return new BigInteger(short.MinValue);
                case IntegerValueSize.Int32:
                    return new BigInteger(int.MinValue);
                case IntegerValueSize.Int64:
                    return new BigInteger(long.MinValue);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static BigInteger GetMaxValue(IntegerValueSize size)
        {
            if (!Enum.IsDefined(typeof(IntegerValueSize), size))
                throw new InvalidEnumArgumentException(nameof(size), (int) size, typeof(IntegerValueSize));
            
            switch (size)
            {
                case IntegerValueSize.Int8:
                    return new BigInteger(sbyte.MaxValue);
                case IntegerValueSize.Int16:
                    return new BigInteger(short.MaxValue);
                case IntegerValueSize.Int32:
                    return new BigInteger(int.MaxValue);
                case IntegerValueSize.Int64:
                    return new BigInteger(long.MaxValue);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override string ToString()
        {
            return Size.ToString("G");
        }

        public bool Equals(IValueType other)
        {
            return Equals((object) other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is IntegerType integerType && Size == integerType.Size;
        }

        public override int GetHashCode()
        {
            return (int) Size;
        }
    }
}