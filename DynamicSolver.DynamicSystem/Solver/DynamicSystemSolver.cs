using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.Abstractions;
using DynamicSolver.Expressions.Execution;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem.Solver
{
    public abstract class DynamicSystemSolver : IDynamicSystemSolver
    {
        protected struct ExecutableFunctionInfo
        {
            public string Name { get; }
            public IExecutableFunction Function { get; }

            public ExecutableFunctionInfo(string name, IExecutableFunction function)
            {
                Name = name;
                Function = function;
            }
        }

        [NotNull] private readonly IExecutableFunctionFactory _functionFactory;
        [NotNull] private readonly ExplicitOrdinaryDifferentialEquationSystem _equationSystem;

        protected DynamicSystemSolver([NotNull] IExecutableFunctionFactory functionFactory, [NotNull] ExplicitOrdinaryDifferentialEquationSystem equationSystem)
        {
            if (functionFactory == null) throw new ArgumentNullException(nameof(functionFactory));
            if (equationSystem == null) throw new ArgumentNullException(nameof(equationSystem));

            if(equationSystem.Equations.Any(e => e.LeadingDerivative.Order > 1))
            {
                throw new ArgumentException($"{nameof(EulerDynamicSystemSolver)} supports only equations with order = 1.");
            }

            _functionFactory = functionFactory;
            _equationSystem = equationSystem;
        }

        public IEnumerable<IDictionary<string, double>> Solve(IDictionary<string, double> initialConditions, double step)
        {
            if (step <= 0) throw new ArgumentOutOfRangeException(nameof(step));
            if (!new HashSet<string>(_equationSystem.Equations.SelectMany(e => e.Function.Analyzer.Variables)).SetEquals(initialConditions.Keys))
            {
                throw new ArgumentException("Initial values has different set of arguments from equation system.");
            }

            var functions = _equationSystem.Equations
                .Select(e => new ExecutableFunctionInfo(e.LeadingDerivative.Variable.Name, _functionFactory.Create(e.Function)))
                .ToList();

            var lastValues = initialConditions;
            while(true)
            {
                yield return lastValues = GetNextValues(functions, lastValues, step);
            }            
        }

        private Dictionary<string, double> GetNextValues(IEnumerable<ExecutableFunctionInfo> functions, IDictionary<string, double> variables, double step)
        {
            var vars = variables.ToDictionary(v => v.Key, v => v.Value);

            foreach (var functionData in functions)
            {
                vars[functionData.Name] = CalculateNextFunctionValue(functionData, variables, step);
            }

            return vars;
        }

        protected abstract double CalculateNextFunctionValue(ExecutableFunctionInfo function, IDictionary<string, double> variables, double step);
    }
}