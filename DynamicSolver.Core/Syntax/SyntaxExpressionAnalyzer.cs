using System;
using System.Collections.Generic;
using DynamicSolver.Core.Syntax.Model;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Syntax
{
    public class SyntaxExpressionAnalyzer
    {
        [NotNull]
        private readonly ISyntaxExpression _expression;

        private IReadOnlyCollection<string> _variables;

        public IReadOnlyCollection<string> Variables => _variables ?? (_variables = (IReadOnlyCollection<string>) GetVariablesSet());

        public SyntaxExpressionAnalyzer([NotNull] ISyntaxExpression expression)
        {
            _expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public ISet<string> GetVariablesSet()
        {
            var set = new HashSet<string>();

            var visitor = new SyntaxExpressionVisitor(_expression);
            visitor.VisitVariablePrimitive += (_, v) => set.Add(v.Name);
            visitor.Visit();

            return set;
        }

        private bool? _isSimpleAssignment;
        public bool IsSimpleAssignment
        {
            get
            {
                if (_isSimpleAssignment.HasValue)
                    return _isSimpleAssignment.Value;

                if (!(_expression is AssignmentBinaryOperator))
                    return (_isSimpleAssignment = false).Value;

                var assignmentsCount = 0;
                var visitor = new SyntaxExpressionVisitor(_expression);
                visitor.VisitAssignmentBinaryOperator += (_, v) => assignmentsCount++;
                visitor.Visit();

                return (_isSimpleAssignment = assignmentsCount == 1).Value;                
            }
        }

        private bool? _isComputable;
        public bool IsComputable
        {
            get
            {
                if (_isComputable.HasValue)
                    return _isComputable.Value;

                var assignmentsCount = 0;
                var visitor = new SyntaxExpressionVisitor(_expression);
                visitor.VisitAssignmentBinaryOperator += (_, v) => assignmentsCount++;
                visitor.Visit();

                return (_isComputable = assignmentsCount == 0).Value;
            }
        }

        public bool HasOperator<T>() where T : ISyntaxExpression
        {
            var result = false;
            var visitor = new SyntaxExpressionVisitor(_expression);
            visitor.VisitAnyNode += (_, v) => result = result || v is T;
            visitor.Visit();
            return result;
        }
    }
}