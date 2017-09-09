using System;
using System.Collections.Generic;
using DynamicSolver.CoreMath.Syntax;
using DynamicSolver.CoreMath.Syntax.Model;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem
{
    public class VariableDerivative : IEquatable<VariableDerivative>
    {
        [NotNull]
        public VariablePrimitive Variable { get; }

        public int Order { get; }

        public VariableDerivative([NotNull] VariablePrimitive variable, int order)
        {
            if (variable == null) throw new ArgumentNullException(nameof(variable));
            if (order < 1) throw new ArgumentOutOfRangeException(nameof(order));

            Variable = variable;
            Order = order;
        }

        public override string ToString()
        {
            return Variable + new string('\'', Order);
        }

        #region Equality members

        private sealed class VariableDerivativeEqualityComparer : IEqualityComparer<VariableDerivative>
        {
            public bool Equals(VariableDerivative x, VariableDerivative y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Variable.Equals(y.Variable) && x.Order == y.Order;
            }

            public int GetHashCode(VariableDerivative obj)
            {
                unchecked
                {
                    return (obj.Variable.GetHashCode()*397) ^ obj.Order;
                }
            }
        }

        public bool Equals(VariableDerivative other)
        {
            return Comparer.Equals(this, other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((VariableDerivative) obj);
        }

        public override int GetHashCode()
        {
            return Comparer.GetHashCode();
        }

        public static bool operator ==(VariableDerivative left, VariableDerivative right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(VariableDerivative left, VariableDerivative right)
        {
            return !Equals(left, right);
        }

        public static IEqualityComparer<VariableDerivative> Comparer { get; } = new VariableDerivativeEqualityComparer();

        #endregion
    }
}