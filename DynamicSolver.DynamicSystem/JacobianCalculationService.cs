using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.Core.Derivation;
using DynamicSolver.Core.Execution;
using DynamicSolver.Core.Syntax.Model;
using DynamicSolver.DynamicSystem.Solvers;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem
{
    public class JacobianCalculationService
    {
        private readonly IExecutableFunctionFactory _functionFactory;

        public JacobianCalculationService([NotNull] IExecutableFunctionFactory functionFactory)
        {
            _functionFactory = functionFactory ?? throw new ArgumentNullException(nameof(functionFactory));
        }

        [NotNull]
        public Dictionary<Tuple<string, string>, ExecutableFunctionInfo> GetJacobianFunctions([NotNull] IReadOnlyCollection<ExplicitOrdinaryDifferentialEquation> equations)
        {
            var nextStateExpressions = ExpressNextStateVariableValueExpressions(equations);

            var dictionary = new Dictionary<Tuple<string, string>, ExecutableFunctionInfo>();

            foreach (var expr in nextStateExpressions)
            {
                foreach (var nameToStatement in expr.Value)
                {
                    var key = Tuple.Create(expr.Key, nameToStatement.Key);
                    var info = new ExecutableFunctionInfo(nameToStatement.Key, _functionFactory.Create(nameToStatement.Value));

                    dictionary.Add(key, info);
                }
            }

            return dictionary;
        }

        [NotNull]
        private IDictionary<string, IDictionary<string, ISyntaxExpression>> ExpressNextStateVariableValueExpressions([NotNull] IReadOnlyCollection<ExplicitOrdinaryDifferentialEquation> equations)
        {
            if (equations == null) throw new ArgumentNullException(nameof(equations));
            
            if (equations.Any(e => e.LeadingDerivative.Order > 1))
            {
                throw new ArgumentException("Equation system contains leading derivatives with order greater than 1", nameof(equations));
            }

            var result = new Dictionary<string, IDictionary<string, ISyntaxExpression>>();

            var derivationService = new SymbolicDerivationService();

            var leadingVariables = equations.Select(e => e.LeadingDerivative).ToList();

            foreach (var equation in equations)
            {
                var derivativeDictionary = new Dictionary<string, ISyntaxExpression>();

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