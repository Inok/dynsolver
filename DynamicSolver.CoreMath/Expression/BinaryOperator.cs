using System;
using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Expression
{
    public abstract class BinaryOperator : IBinaryOperator, IEquatable<IBinaryOperator>
    {
        public abstract string OperatorToken { get; }
        
        public IExpression LeftOperand { get; }
        public IExpression RightOperand { get; }

        protected BinaryOperator([NotNull] IExpression leftOperand, [NotNull] IExpression rightOperand)
        {
            if (leftOperand == null) throw new ArgumentNullException(nameof(leftOperand));
            if (rightOperand == null) throw new ArgumentNullException(nameof(rightOperand));

            LeftOperand = leftOperand;
            RightOperand = rightOperand;
        }

        public override string ToString()
        {
            return $"({LeftOperand}) {OperatorToken} ({RightOperand})";
        }

        public bool Equals(IBinaryOperator other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(OperatorToken, other.OperatorToken, StringComparison.Ordinal)
                   && LeftOperand.Equals(other.LeftOperand)
                   && RightOperand.Equals(other.RightOperand);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as IBinaryOperator;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (OperatorToken.GetHashCode()*397 ^ LeftOperand.GetHashCode())*397 ^ RightOperand.GetHashCode();
            }
        }
    }
}