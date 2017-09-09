using System;
using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Syntax
{
    public sealed class UnaryMinusOperator : IUnaryOperator, IEquatable<UnaryMinusOperator>
    {
        public IExpression Operand { get; }

        public UnaryMinusOperator([NotNull] IExpression operand)
        {
            if (operand == null) throw new ArgumentNullException(nameof(operand));

            Operand = operand;
        }

        public override string ToString()
        {
            return $"-({Operand})";
        }

        public bool Equals(UnaryMinusOperator other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Operand.Equals(other.Operand);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is UnaryMinusOperator && Equals((UnaryMinusOperator) obj);
        }

        public bool Equals(IExpression other)
        {
            return this.Equals((object)other);
        }

        public override int GetHashCode()
        {
            return Operand.GetHashCode();
        }
    }
}