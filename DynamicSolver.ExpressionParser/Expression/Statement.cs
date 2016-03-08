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
    }
}