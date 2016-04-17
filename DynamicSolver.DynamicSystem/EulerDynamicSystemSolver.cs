using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.Abstractions;
using DynamicSolver.Expressions.Execution.Interpreter;
using DynamicSolver.Expressions.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem
{
    public class EulerDynamicSystemSolver : IDynamicSystemSolver
    {
        [NotNull]
        private readonly ExplicitOrdinaryDifferentialEquationSystem _equationSystem;

        public EulerDynamicSystemSolver([NotNull] ExplicitOrdinaryDifferentialEquationSystem equationSystem)
        {
            if (equationSystem == null) throw new ArgumentNullException(nameof(equationSystem));

            if(equationSystem.Equations.Any(e => e.LeadingDerivative.Order > 1))
            {
                throw new ArgumentException($"{nameof(EulerDynamicSystemSolver)} supports only equations with order = 1.");
            }

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
                .Select(e => new Tuple<VariablePrimitive, IExecutableFunction>(e.LeadingDerivative.Variable, new InterpretedFunction(e.Function)))
                .ToList();

            var lastValues = initialConditions;
            while(true)
            {
                yield return lastValues = GetNextValues(functions, lastValues, step);
            }            
        }

        private static Dictionary<string, double> GetNextValues(IEnumerable<Tuple<VariablePrimitive, IExecutableFunction>> functions, IDictionary<string, double> variables, double step)
        {
            var vars = variables.ToDictionary(v => v.Key, v => v.Value);

            foreach (var function in functions)
            {
                var arguments = function.Item2.OrderedArguments.Select(a => variables[a]).ToArray();
                vars[function.Item1.Name] = variables[function.Item1.Name] + step*function.Item2.Execute(arguments);
            }

            return vars;
        }
    }
}