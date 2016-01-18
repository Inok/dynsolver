using System;
using DynamicSolver.Abstractions.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.ExpressionParser.Expression
{
    public abstract class BinaryOperator : IBinaryOperator
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

    }
}