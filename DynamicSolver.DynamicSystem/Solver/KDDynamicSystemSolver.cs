using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.Expressions.Execution;
using DynamicSolver.Expressions.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem.Solver
{
    public class KDDynamicSystemSolver : IDynamicSystemSolver
    {
        [NotNull]
        private readonly IExecutableFunctionFactory _functionFactory;

        public KDDynamicSystemSolver([NotNull] IExecutableFunctionFactory functionFactory)
        {
            if (functionFactory == null) throw new ArgumentNullException(nameof(functionFactory));            
            _functionFactory = functionFactory;
        }

        public IEnumerable<IReadOnlyDictionary<string, double>> Solve(ExplicitOrdinaryDifferentialEquationSystem equationSystem, IReadOnlyDictionary<string, double> initialConditions, double step)
        {
            if (equationSystem == null) throw new ArgumentNullException(nameof(equationSystem));
            if (initialConditions == null) throw new ArgumentNullException(nameof(initialConditions));
            if (step <= 0) throw new ArgumentOutOfRangeException(nameof(step));

            if (equationSystem.Equations.Any(e => e.LeadingDerivative.Order > 1))
            {
                throw new ArgumentException($"{nameof(EulerDynamicSystemSolver)} supports only equations with order = 1.");
            }
            if (!new HashSet<string>(equationSystem.Equations.SelectMany(e => e.Function.Analyzer.Variables)).SetEquals(initialConditions.Keys))
            {
                throw new ArgumentException("Initial values has different set of arguments from equation system.");
            }

            var functions = equationSystem.Equations
                .Select(e => new Tuple<VariablePrimitive, IExecutableFunction>(e.LeadingDerivative.Variable, _functionFactory.Create(e.Function)))
                .ToList();

            var service = new NextStateVariableValueCalculationService();
            var nextValuesFunctions = service.ExpressNextStateVariableValueExpressions(equationSystem, "h")
                .ToDictionary(p => p.Key, p => _functionFactory.Create(p.Value));

            var lastValues = initialConditions;
            while (true)
            {
                yield return lastValues = GetNextValues(functions, lastValues, nextValuesFunctions, step);
            }
        }

        private static IReadOnlyDictionary<string, double> GetNextValues(IList<Tuple<VariablePrimitive, IExecutableFunction>> functions, IReadOnlyDictionary<string, double> variables, IDictionary<string, IExecutableFunction> nextValuesFunctions, double step)
        {
            var halfStep = step/2;

            var firstHalfVars = variables.ToDictionary(v => v.Key, v => v.Value);

            for (int i = 0; i < functions.Count; i++)
            {
                var function = functions[i];
                var arguments = function.Item2.OrderedArguments.Select(a =>
                    functions.Select(f => f.Item1.Name).Take(i + 1).Contains(a)
                        ? nextValuesFunctions[a].Execute(nextValuesFunctions[a].OrderedArguments.Select(t => t == "h" ? halfStep : variables[t]).ToArray())
                        : variables[a]).ToArray();

                firstHalfVars[function.Item1.Name] = variables[function.Item1.Name] + halfStep * function.Item2.Execute(arguments);
            }

            var secondHalfVars = firstHalfVars.ToDictionary(v => v.Key, v => v.Value);
            for (int i = functions.Count - 1; i >= 0; i--)
            {
                var function = functions[i];
                var arguments = function.Item2.OrderedArguments.Select(a =>
                    functions.Select(f => f.Item1.Name).Skip(i + 1).Contains(a)
                        ? nextValuesFunctions[a].Execute(nextValuesFunctions[a].OrderedArguments.Select(t => t == "h" ? halfStep : firstHalfVars[t]).ToArray())
                        : firstHalfVars[a]).ToArray();

                secondHalfVars[function.Item1.Name] = firstHalfVars[function.Item1.Name] + halfStep * function.Item2.Execute(arguments);
            }

            return secondHalfVars;
        }
    }
}