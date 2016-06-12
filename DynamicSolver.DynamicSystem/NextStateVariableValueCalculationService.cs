using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.Expressions.Derivation;
using DynamicSolver.Expressions.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem
{
    public class NextStateVariableValueCalculationService
    {
        [NotNull]
        public IDictionary<string, IStatement> ExpressNextStateVariableValueExpressions([NotNull] ExplicitOrdinaryDifferentialEquationSystem equationSystem, [NotNull] string stepVariableName)
        {
            if (equationSystem == null) throw new ArgumentNullException(nameof(equationSystem));
            if (string.IsNullOrWhiteSpace(stepVariableName)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(stepVariableName));

            if (equationSystem.Equations.Any(e => e.LeadingDerivative.Variable.Name == stepVariableName || e.Function.Analyzer.Variables.Contains(stepVariableName)))
            {
                throw new ArgumentException($"Equation system contains variable with name equal to stepVariableName = {stepVariableName}", nameof(stepVariableName));
            }

            if (equationSystem.Equations.Any(e => e.LeadingDerivative.Order > 1))
            {
                throw new ArgumentException("Equation system contains leading derivatives with order greater than 1", nameof(stepVariableName));
            }

            var jacobian = GetJacobian(equationSystem);

            var result = new Dictionary<string, IStatement>();
            foreach (var equationDerivatives in jacobian)
            {
                var derivativeMatrixRow = equationDerivatives.Value.Select(e => new
                {
                    variable = e.Key,
                    expr = (IExpression) new SubtractBinaryOperator(
                        new NumericPrimitive(equationDerivatives.Key == e.Key ? "1" : "0"),
                        new MultiplyBinaryOperator(new VariablePrimitive(stepVariableName), e.Value))
                });

                IExpression expression = null;
                foreach (var rowItem in derivativeMatrixRow)
                {
                    var item = new MultiplyBinaryOperator(rowItem.expr, new VariablePrimitive(rowItem.variable));
                    if (expression == null)
                    {
                        expression = item;
                    }
                    else
                    {
                        expression = new AddBinaryOperator(expression, item);
                    }
                }
                result.Add(equationDerivatives.Key, new Statement(expression));
            }

            return result;
        }

        private static IDictionary<string, IDictionary<string, IExpression>> GetJacobian(ExplicitOrdinaryDifferentialEquationSystem equationSystem)
        {
            var result = new Dictionary<string, IDictionary<string, IExpression>>();

            var derivationService = new SymbolicDerivationService();

            var leadingVariables = equationSystem.Equations.Select(e => e.LeadingDerivative).ToList();

            foreach (var equation in equationSystem.Equations)
            {
                var derivativeDictionary = new Dictionary<string, IExpression>();

                foreach (var leading in leadingVariables)
                {
                    derivativeDictionary[leading.Variable.Name] = derivationService.GetDerivative(equation.Function, leading.Variable.Name).Expression;
                }

                result[equation.LeadingDerivative.Variable.Name] = derivativeDictionary;
            }

            return result;
        }
    }
}