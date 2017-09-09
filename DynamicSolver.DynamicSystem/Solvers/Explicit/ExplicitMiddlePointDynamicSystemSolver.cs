using System;
using System.Collections.Generic;
using DynamicSolver.DynamicSystem.Step;

namespace DynamicSolver.DynamicSystem.Solvers.Explicit
{
    public class ExplicitMiddlePointDynamicSystemSolver : IDynamicSystemSolver
    {
        public DynamicSystemSolverDescription Description { get; } = new DynamicSystemSolverDescription("Explicit middle point", 2, false);

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

                var middlePointState = new Dictionary<string, double>();

                foreach (var function in functions)
                {
                    middlePointState[function.Name] = lastState.DependentVariables[function.Name] + 0.5 * step.Delta * function.Execute(lastState.DependentVariables);
                }

                var dependentVariables = new Dictionary<string, double>();
                foreach (var function in functions)
                {
                    dependentVariables[function.Name] = lastState.DependentVariables[function.Name] + step.Delta * function.Execute(middlePointState);
                }

                lastState = new DynamicSystemState(step.AbsoluteValue, dependentVariables);

                yield return lastState;
            }
        }
    }
}