using System;
using System.Collections.Generic;
using DynamicSolver.Modelling.Step;

namespace DynamicSolver.Modelling.Solvers.Explicit
{
    public class ExplicitEulerSolver : IDynamicSystemSolver
    {
        public DynamicSystemSolverDescription Description { get; } = new DynamicSystemSolverDescription("Euler", 1, false);

        public IEnumerable<DynamicSystemState> Solve(IExplicitOrdinaryDifferentialEquationSystem equationSystem,
            DynamicSystemState initialState,
            ModellingTaskParameters parameters)
        {
            if (equationSystem == null) throw new ArgumentNullException(nameof(equationSystem));
            if (initialState == null) throw new ArgumentNullException(nameof(initialState));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            var functions = equationSystem.ExecutableFunctions;
            var stepper = new FixedStepStepper(parameters.Step, initialState.IndependentVariable);

            var lastState = initialState;
            while (true)
            {
                var step = stepper.MoveNext();

                var dependentVariables = new Dictionary<string, double>();

                foreach (var function in functions)
                {
                    var value = lastState.DependentVariables[function.Name] + step.Delta * function.Execute(lastState.DependentVariables);
                    dependentVariables[function.Name] = value;
                }

                lastState = new DynamicSystemState(step.AbsoluteValue, dependentVariables);

                yield return lastState;
            }
        }
    }
}