using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.DynamicSystem.Step;

namespace DynamicSolver.DynamicSystem.Solvers.SemiImplicit
{
    public class KDFastDynamicSystemSolver : IDynamicSystemSolver
    {
        public DynamicSystemSolverDescription Description { get; } = new DynamicSystemSolverDescription("KD (fast)", 2, true);

        public IEnumerable<DynamicSystemState> Solve(IExplicitOrdinaryDifferentialEquationSystem equationSystem,
            DynamicSystemState initialState,
            ModellingTaskParameters parameters)
        {
            if (equationSystem == null) throw new ArgumentNullException(nameof(equationSystem));
            if (initialState == null) throw new ArgumentNullException(nameof(initialState));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            var functions = equationSystem.ExecutableFunctions;
            var stepper = new FixedStepStepper(parameters.Step, initialState.IndependentVariable);

            var fourStepFunctions = functions.Select(f => f.Function.OrderedArguments.Contains(f.Name)).ToArray();

            var firstHalfVars = new Dictionary<string, double>();
            
            var lastState = initialState;
            while (true)
            {
                var step = stepper.MoveNext();

                var halfStep = step.Delta / 2;

                firstHalfVars.Clear();
                lastState.DependentVariables.CopyTo(firstHalfVars);
                
                foreach (var function in functions)
                {
                    var value = firstHalfVars[function.Name] + halfStep * function.Execute(firstHalfVars);
                    firstHalfVars[function.Name] = value;
                }

                var secondHalfVars = ((IDictionary<string, double>) firstHalfVars).Clone();

                for (var i = functions.Count - 1; i >= 0; i--)
                {
                    var function = functions[i];

                    if (fourStepFunctions[i])
                    {
                        var startValue = firstHalfVars[function.Name];
                        secondHalfVars[function.Name] = startValue + halfStep * function.Function.Execute(secondHalfVars);
                        secondHalfVars[function.Name] = startValue + halfStep * function.Function.Execute(secondHalfVars);
                        secondHalfVars[function.Name] = startValue + halfStep * function.Function.Execute(secondHalfVars);
                        secondHalfVars[function.Name] = startValue + halfStep * function.Function.Execute(secondHalfVars);
                    }
                    else
                    {
                        secondHalfVars[function.Name] = secondHalfVars[function.Name] + halfStep * function.Function.Execute(secondHalfVars);
                    }
                }

                yield return lastState = new DynamicSystemState(step.AbsoluteValue, secondHalfVars);
            }
        }
    }
}