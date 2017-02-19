using System;
using System.Linq;
using DynamicSolver.Expressions.Analysis;
using DynamicSolver.Expressions.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem
{
    public class ExplicitOrdinaryDifferentialEquation
    {
        private readonly IStatement _originalStatement;

        [NotNull]
        private static readonly ExpressionFormatter Formatter = new ExpressionFormatter();

        public VariableDerivative LeadingDerivative { get; }
        public IStatement Function { get; }

        private ExplicitOrdinaryDifferentialEquation(IStatement originalStatement, VariableDerivative leadingDerivative, IStatement function)
        {
            _originalStatement = originalStatement;

            LeadingDerivative = leadingDerivative;
            Function = function;
        }

        public override string ToString()
        {
            return Formatter.Format(_originalStatement.Expression);
        }

        public static ExplicitOrdinaryDifferentialEquation FromStatement([NotNull] IStatement statement)
        {
            if (statement == null) throw new ArgumentNullException(nameof(statement));
            
            if (!new ExpressionAnalyzer(statement).IsSimpleAssignment)
            {
                throw new FormatException("Statement is not simple assignment.");
            }

            var assignmentOperator = (AssignmentBinaryOperator) statement.Expression;

            var leftOperand = assignmentOperator.LeftOperand;
            var leftDerivativeAnalyzer = new DerivativeAnalyzer(leftOperand);

            if (!leftDerivativeAnalyzer.IsVariableDerivative)
            {
                throw new FormatException($"Variable derivative expected at left side of statement, but was {new Statement(leftOperand)}.");
            }
            var leadingDerivative = leftDerivativeAnalyzer.AsVariableDerivative();

            var rightOperand = assignmentOperator.RightOperand;
            var rightDerivativeAnalyzer = new DerivativeAnalyzer(rightOperand);

            if(!rightDerivativeAnalyzer.HasVariableDerivativesOnly)
            {
                throw new FormatException("Right side of statement should have only derivatives of variables.");
            }

            var derivatives = rightDerivativeAnalyzer.AllVariableDerivatives();
            var invalidDerivative = derivatives.FirstOrDefault(d => d.Variable.Equals(leadingDerivative.Variable) && d.Order >= leadingDerivative.Order);
            if (invalidDerivative != null)
            {
                throw new FormatException($"Expression contains derivative that has order greater than or equal to leading derivative order: {invalidDerivative}");
            }

            return new ExplicitOrdinaryDifferentialEquation(statement, leadingDerivative, new Statement(rightOperand));
        }
    }
}