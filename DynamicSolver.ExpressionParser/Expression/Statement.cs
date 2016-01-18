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

        public Statement([NotNull] IExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            Expression = expression;
        }

        public override string ToString()
        {
            return Formatter.Format(Expression);
        }
    }
}