using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.Abstractions;
using DynamicSolver.Expressions.Execution.Interpreter;
using DynamicSolver.Expressions.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem
{
    public class KDDynamicSystemSolver : IDynamicSystemSolver
    {
        [NotNull]
        private readonly ExplicitOrdinaryDifferentialEquationSystem _equationSystem;

        public KDDynamicSystemSolver([NotNull] ExplicitOrdinaryDifferentialEquationSystem equationSystem)
        {
            if (equationSystem == null) throw new ArgumentNullException(nameof(equationSystem));

            if (equationSystem.Equations.Any(e => e.LeadingDerivative.Order > 1))
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

            var service = new NextStateVariableValueCalculationService();
            var nextValuesFunctions = service.ExpressNextStateVariableValueExpressions(_equationSystem, "h")
                .ToDictionary(p => p.Key, p => (IExecutableFunction)new InterpretedFunction(p.Value));

            var lastValues = initialConditions;
            while (true)
            {
                yield return lastValues = GetNextValues(functions, lastValues, nextValuesFunctions, step);
            }
        }

        private static Dictionary<string, double> GetNextValues(IList<Tuple<VariablePrimitive, IExecutableFunction>> functions, IDictionary<string, double> variables, IDictionary<string, IExecutableFunction> nextValuesFunctions, double step)
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