using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.DynamicSystem.Step;

namespace DynamicSolver.DynamicSystem.Solvers.Explicit
{
    public abstract class ButcherTableDynamicSystemSolver : IDynamicSystemSolver
    {
        public abstract DynamicSystemSolverDescription Description { get; }

        protected abstract double[,] A { get; }
        protected abstract double[] B { get; }


        public IEnumerable<DynamicSystemState> Solve(IExplicitOrdinaryDifferentialEquationSystem equationSystem, 
            DynamicSystemState initialState,
            ModellingTaskParameters parameters)
        {
            if (equationSystem == null) throw new ArgumentNullException(nameof(equationSystem));
            if (initialState == null) throw new ArgumentNullException(nameof(initialState));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            var functions = equationSystem.ExecutableFunctions;
            var stepper = new FixedStepStepper(parameters.Step, initialState.IndependentVariable);

            var functionNameToIndex = functions.Select((f, i) => new { f, i }).ToDictionary(p => p.f.Name, p => p.i);

            var lastState = initialState;
            while (true)
            {
                var step = stepper.MoveNext();

                var k = new double[B.Length, functions.Count];
                var arguments = new double[functions.Count];

                for (var s = 0; s < B.Length; s++)
                {
                    for (var f = 0; f < functions.Count; f++)
                    {
                        var name = functions[f].Name;
                        var sum = 0.0;
                        for (var b = 0; b < s; b++)
                        {
                            sum += A[s, b] * k[b, f];
                        }

                        arguments[f] = lastState.DependentVariables[name] + step.Delta * sum;
                    }

                    for (var f = 0; f < functions.Count; f++)
                    {
                        var function = functions[f].Function;
                        var args = function.OrderedArguments.Select(a => arguments[functionNameToIndex[a]]).ToArray();
                        k[s, f] = function.Execute(args);
                    }
                }

                var dependentVariables = new Dictionary<string, double>();

                for (var f = 0; f < functions.Count; f++)
                {
                    var function = functions[f];

                    dependentVariables[function.Name] = lastState.DependentVariables[function.Name] + step.Delta * B.Select((b, i) => k[i, f] * b).Sum();
                }

                lastState = new DynamicSystemState(step.AbsoluteValue, dependentVariables);
                yield return lastState;
            }
        }
    }
}