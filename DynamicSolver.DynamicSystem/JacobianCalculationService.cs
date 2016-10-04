using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.Expressions.Derivation;
using DynamicSolver.Expressions.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem
{
    public class JacobianCalculationService
    {
        [NotNull]
        public IDictionary<string, IDictionary<string, IStatement>> ExpressNextStateVariableValueExpressions([NotNull] ExplicitOrdinaryDifferentialEquationSystem equationSystem)
        {
            if (equationSystem == null) throw new ArgumentNullException(nameof(equationSystem));
            
            if (equationSystem.Equations.Any(e => e.LeadingDerivative.Order > 1))
            {
                throw new ArgumentException("Equation system contains leading derivatives with order greater than 1", nameof(equationSystem));
            }

            var result = new Dictionary<string, IDictionary<string, IStatement>>();

            var derivationService = new SymbolicDerivationService();

            var leadingVariables = equationSystem.Equations.Select(e => e.LeadingDerivative).ToList();

            foreach (var equation in equationSystem.Equations)
            {
                var derivativeDictionary = new Dictionary<string, IStatement>();

                foreach (var leading in leadingVariables)
                {
                    derivativeDictionary[leading.Variable.Name] = derivationService.GetDerivative(equation.Function, leading.Variable.Name);
                }

                result[equation.LeadingDerivative.Variable.Name] = derivativeDictionary;
            }

            return result;
        }
    }
}