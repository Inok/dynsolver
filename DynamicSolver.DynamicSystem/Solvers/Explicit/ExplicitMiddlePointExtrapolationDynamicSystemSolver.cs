using System;
using System.Collections.Generic;
using DynamicSolver.DynamicSystem.Step;

namespace DynamicSolver.DynamicSystem.Solvers.Explicit
{
    public class ExplicitMiddlePointExtrapolationDynamicSystemSolver : IDynamicSystemSolver
    {
        public DynamicSystemSolverDescription Description { get; } = new DynamicSystemSolverDescription("Explicit middle point (extrapolation)", 2, true);

        public IEnumerable<DynamicSystemState> Solve(IExplicitOrdinaryDifferentialEquationSystem equationSystem, IIndependentVariableStepStrategy stepStrategy)
        {
            if (equationSystem == null) throw new ArgumentNullException(nameof(equationSystem));
            if (stepStrategy == null) throw new ArgumentNullException(nameof(stepStrategy));

            var functions = equationSystem.ExecutableFunctions;
            var stepper = stepStrategy.Create(equationSystem.InitialState.IndependentVariable);

            var previousState = equationSystem.InitialState.DependentVariables;
            var step = stepper.MoveNext();

            var middlePointState = new Dictionary<string, double>();

            foreach (var function in functions)
            {
                middlePointState[function.Name] = previousState[function.Name] + step.Delta * function.Execute(previousState);
            }

            while (true)
            {
                var dependentVariables = new Dictionary<string, double>();
                foreach (var function in functions)
                {
                    dependentVariables[function.Name] = previousState[function.Name] + 2*step.Delta * function.Execute(middlePointState);
                }

                yield return new DynamicSystemState(step.AbsoluteValue + step.Delta, dependentVariables);

                previousState = middlePointState;
                middlePointState = dependentVariables;

                step = stepper.MoveNext();
            }
        }
    }
}