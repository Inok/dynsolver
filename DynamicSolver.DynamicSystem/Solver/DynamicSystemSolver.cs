using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.Expressions.Execution;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem.Solver
{
    public abstract class DynamicSystemSolver<TStepExtraArguments> : IDynamicSystemSolver
    {
        [NotNull] protected IExecutableFunctionFactory FunctionFactory { get; }

        protected DynamicSystemSolver(IExecutableFunctionFactory functionFactory)
        {
            if (functionFactory == null) throw new ArgumentNullException(nameof(functionFactory));
            FunctionFactory = functionFactory;
        }

        public IEnumerable<IReadOnlyDictionary<string, double>> Solve(ExplicitOrdinaryDifferentialEquationSystem equationSystem, IReadOnlyDictionary<string, double> initialConditions, double step)
        {
            if (equationSystem == null) throw new ArgumentNullException(nameof(equationSystem));
            if (initialConditions == null) throw new ArgumentNullException(nameof(initialConditions));
            if (step <= 0) throw new ArgumentOutOfRangeException(nameof(step));

            if (equationSystem.Equations.Any(e => e.LeadingDerivative.Order > 1))
            {
                throw new ArgumentException($"{GetType().Name} supports only equations with order = 1.");
            }
            if (!new HashSet<string>(equationSystem.Equations.SelectMany(e => e.Function.Analyzer.Variables)).SetEquals(initialConditions.Keys))
            {
                throw new ArgumentException("Initial values has different set of arguments from equation system.");
            }

            var functions = equationSystem.Equations
                .Select(e => new ExecutableFunctionInfo(e.LeadingDerivative.Variable.Name, FunctionFactory.Create(e.Function)))
                .ToList();

            var extra = GetExtraArguments(equationSystem, functions, initialConditions, step);

            var lastValues = initialConditions;
            while(true)
            {
                yield return lastValues = GetNextValues(functions, lastValues, step, extra);
            }            
        }

        protected virtual TStepExtraArguments GetExtraArguments(ExplicitOrdinaryDifferentialEquationSystem equationSystem, IList<ExecutableFunctionInfo> functions, IReadOnlyDictionary<string, double> initialConditions, double step)
        {
            return default(TStepExtraArguments);
        }

        protected abstract IReadOnlyDictionary<string, double> GetNextValues(IList<ExecutableFunctionInfo> functions, IReadOnlyDictionary<string, double> variables, double step, TStepExtraArguments extra);
    }
}