using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.CoreMath.Derivation;
using DynamicSolver.CoreMath.Expression;
using DynamicSolver.DynamicSystem.Solvers;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem
{
    public class JacobianCalculationService
    {
        [NotNull]
        public Dictionary<Tuple<string, string>, ExecutableFunctionInfo> GetJacobianFunctions([NotNull] IExplicitOrdinaryDifferentialEquationSystem equationSystem)
        {
            var nextStateExpressions = ExpressNextStateVariableValueExpressions(equationSystem);

            var dictionary = new Dictionary<Tuple<string, string>, ExecutableFunctionInfo>();

            foreach (var expr in nextStateExpressions)
            {
                foreach (var nameToStatement in expr.Value)
                {
                    var key = Tuple.Create(expr.Key, nameToStatement.Key);
                    var info = new ExecutableFunctionInfo(nameToStatement.Key, equationSystem.FunctionFactory.Create(nameToStatement.Value));

                    dictionary.Add(key, info);
                }
            }

            return dictionary;
        }

        [NotNull]
        public IDictionary<string, IDictionary<string, IExpression>> ExpressNextStateVariableValueExpressions([NotNull] IExplicitOrdinaryDifferentialEquationSystem equationSystem)
        {
            if (equationSystem == null) throw new ArgumentNullException(nameof(equationSystem));
            
            if (equationSystem.Equations.Any(e => e.LeadingDerivative.Order > 1))
            {
                throw new ArgumentException("Equation system contains leading derivatives with order greater than 1", nameof(equationSystem));
            }

            var result = new Dictionary<string, IDictionary<string, IExpression>>();

            var derivationService = new SymbolicDerivationService();

            var leadingVariables = equationSystem.Equations.Select(e => e.LeadingDerivative).ToList();

            foreach (var equation in equationSystem.Equations)
            {
                var derivativeDictionary = new Dictionary<string, IExpression>();

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