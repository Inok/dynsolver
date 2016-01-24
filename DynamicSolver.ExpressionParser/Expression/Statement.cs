using System;
using DynamicSolver.Abstractions.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.ExpressionParser.Expression
{
    public sealed class Statement : IStatement
    {
        [NotNull]
        private static readonly ExpressionFormatter Formatter = new ExpressionFormatter();

        public IExpression Expression { get; }

        public bool IsEquation
        {
            get
            {
                var equality = Expression as EqualityBinaryOperator;
                return equality != null && equality.LeftOperand is IVariablePrimitive;
            }
        }

        public Statement([NotNull] IExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            Expression = expression;
        }

        public IEquation ToEquation()
        {
            var equality = Expression as EqualityBinaryOperator;
            if (equality != null && equality.LeftOperand is IVariablePrimitive)
            {
                return new Equation((IVariablePrimitive) equality.LeftOperand, equality.RightOperand);
            }

            throw new InvalidOperationException("This statement is not an equation: " + Formatter.Format(Expression));
        }

        public override string ToString()
        {
            return Formatter.Format(Expression);
        }
    }
}