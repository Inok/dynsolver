using System;
using System.Collections.Generic;
using DynamicSolver.Abstractions;
using DynamicSolver.Abstractions.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.ExpressionParser.Tools
{
    public class ExpressionAnalyzer : IExpressionAnalyzer
    {
        [NotNull]
        private readonly IExpression _expression;

        private IReadOnlyCollection<string> _variables;

        public IReadOnlyCollection<string> Variables => _variables ?? (_variables = (IReadOnlyCollection<string>) GetVariablesSet());

        public ExpressionAnalyzer([NotNull] IExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            _expression = expression;
        }

        public ISet<string> GetVariablesSet()
        {
            var set = new HashSet<string>();

            var visitor = new ExpressionVisitor(_expression);
            visitor.VisitVariablePrimitive += (_, v) => set.Add(v.Name);
            visitor.Visit();

            return set;
        }
    }
}