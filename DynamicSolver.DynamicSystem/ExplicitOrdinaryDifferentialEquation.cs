using System;
using System.Linq;
using DynamicSolver.CoreMath.Analysis;
using DynamicSolver.CoreMath.Syntax;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem
{
    public class ExplicitOrdinaryDifferentialEquation
    {
        private readonly IExpression _originalExpression;

        [NotNull]
        private static readonly ExpressionFormatter Formatter = new ExpressionFormatter();

        public VariableDerivative LeadingDerivative { get; }
        public IExpression Function { get; }

        private ExplicitOrdinaryDifferentialEquation(IExpression originalExpression, VariableDerivative leadingDerivative, IExpression function)
        {
            _originalExpression = originalExpression;

            LeadingDerivative = leadingDerivative;
            Function = function;
        }

        public override string ToString()
        {
            return Formatter.Format(_originalExpression);
        }

        public static ExplicitOrdinaryDifferentialEquation FromExpression([NotNull] IExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            
            if (!new ExpressionAnalyzer(expression).IsSimpleAssignment)
            {
                throw new FormatException("Statement is not simple assignment.");
            }

            var assignmentOperator = (AssignmentBinaryOperator) expression;

            var leftOperand = assignmentOperator.LeftOperand;
            var leftDerivativeAnalyzer = new DerivativeAnalyzer(leftOperand);

            if (!leftDerivativeAnalyzer.IsVariableDerivative)
            {
                var formatter = new ExpressionFormatter();
                throw new FormatException($"Variable derivative expected at left side of expression, but was {formatter.Format(leftOperand)}.");
            }
            var leadingDerivative = leftDerivativeAnalyzer.AsVariableDerivative();

            var rightOperand = assignmentOperator.RightOperand;
            var rightDerivativeAnalyzer = new DerivativeAnalyzer(rightOperand);

            if(!rightDerivativeAnalyzer.HasVariableDerivativesOnly)
            {
                throw new FormatException("Right side of expression should have only derivatives of variables.");
            }

            var derivatives = rightDerivativeAnalyzer.AllVariableDerivatives();
            var invalidDerivative = derivatives.FirstOrDefault(d => d.Variable.Equals(leadingDerivative.Variable) && d.Order >= leadingDerivative.Order);
            if (invalidDerivative != null)
            {
                throw new FormatException($"Expression contains derivative that has order greater than or equal to leading derivative order: {invalidDerivative}");
            }

            return new ExplicitOrdinaryDifferentialEquation(expression, leadingDerivative, rightOperand);
        }
    }
}