using System;
using System.Collections.Generic;
using DynamicSolver.DynamicSystem.Step;

namespace DynamicSolver.DynamicSystem.Solvers.Explicit
{
    public class EulerCromerSolver : IDynamicSystemSolver
    {
        public DynamicSystemSolverDescription Description { get; } = new DynamicSystemSolverDescription("Euler-Cromer", 1, false);

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

                var dependentVariables = lastState.DependentVariables.Clone();
                
                foreach (var function in functions)
                {
                    var value = dependentVariables[function.Name] + step.Delta * function.Execute(dependentVariables);
                    dependentVariables[function.Name] = value;
                }

                lastState = new DynamicSystemState(step.AbsoluteValue, dependentVariables);

                yield return lastState;
            }
        }
    }
}