using System;
using DynamicSolver.Abstractions;
using DynamicSolver.Abstractions.Expression;
using DynamicSolver.ExpressionParser.Tools;
using JetBrains.Annotations;

namespace DynamicSolver.ExpressionParser.Expression
{
    public sealed class Statement : IStatement
    {
        [NotNull]
        private static readonly ExpressionFormatter Formatter = new ExpressionFormatter();

        public IExpression Expression { get; }

        private IExpressionAnalyzer _analyzer;
        public IExpressionAnalyzer Analyzer => _analyzer ?? (_analyzer = new ExpressionAnalyzer(Expression));

        public Statement([NotNull] IExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            Expression = expression;
        }

        public override string ToString()
        {
            return Formatter.Format(Expression);
        }

        public bool Equals(IStatement other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Expression.Equals(other.Expression);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Statement && Equals((Statement) obj);
        }

        public override int GetHashCode()
        {
            return Expression.GetHashCode();
        }
    }
}