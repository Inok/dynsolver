using System;
using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Expression
{
    public class ConstantPrimitive : IPrimitive, IEquatable<ConstantPrimitive>
    {
        [NotNull]
        public Constant Constant { get; }

        public ConstantPrimitive([NotNull] Constant constant)
        {
            if (constant == null) throw new ArgumentNullException(nameof(constant));
            Constant = constant;
        }

        public override string ToString()
        {
            return $"{Constant.Name}";
        }

        public bool Equals(ConstantPrimitive other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Constant.Equals(other.Constant);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ConstantPrimitive) obj);
        }

        public bool Equals(IExpression other)
        {
            return this.Equals((object)other);
        }

        public override int GetHashCode()
        {
            return Constant.GetHashCode();
        }
    }
}