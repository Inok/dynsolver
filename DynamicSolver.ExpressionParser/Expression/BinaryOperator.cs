using System;
using DynamicSolver.Abstractions.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.ExpressionParser.Expression
{
    public abstract class BinaryOperator : IBinaryOperator
    {
        private static readonly BinaryOperatorsPriorityComparer PriorityComparer = new BinaryOperatorsPriorityComparer();

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
            return $"{FormatOperand(LeftOperand)} {OperatorToken} {FormatOperand(RightOperand)}";
        }

        private string FormatOperand(IExpression operand)
        {
            var binary = operand as IBinaryOperator;

            if (binary == null)
            {
                return operand.ToString();
            }

            if (binary is PowBinaryOperator)
            {
                return $"({operand})";
            }

            var compare = PriorityComparer.Compare(this, binary);
            if (compare > 0)
            {
                return $"({operand})";
            }

            if (compare == 0)
            {
                if (binary is DivideBinaryOperator)
                {
                    return $"({operand})";
                }

                if (binary is MultiplyBinaryOperator && !(this is MultiplyBinaryOperator))
                {
                    return $"({operand})";
                }
            }

            return operand.ToString();
        }
    }
}