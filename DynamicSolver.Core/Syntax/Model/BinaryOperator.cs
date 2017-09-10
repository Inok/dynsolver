using System;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Syntax.Model
{
    public abstract class BinaryOperator : IBinaryOperator, IEquatable<IBinaryOperator>
    {
        protected abstract string OperatorToken { get; }

        public ISyntaxExpression LeftOperand { get; }
        public ISyntaxExpression RightOperand { get; }

        protected BinaryOperator([NotNull] ISyntaxExpression leftOperand, [NotNull] ISyntaxExpression rightOperand)
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
            return this.GetType() == other.GetType()
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

        public bool Equals(ISyntaxExpression other)
        {
            return this.Equals((object)other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (OperatorToken.GetHashCode() * 397 ^ LeftOperand.GetHashCode()) * 397 ^ RightOperand.GetHashCode();
            }
        }
    }
}