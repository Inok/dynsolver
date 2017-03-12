using System;
using System.Collections.Generic;
using DynamicSolver.DynamicSystem.Step;

namespace DynamicSolver.DynamicSystem.Solvers.Explicit
{
    public class ExplicitMiddlePointDynamicSystemSolver : IDynamicSystemSolver
    {
        public DynamicSystemSolverDescription Description { get; } = new DynamicSystemSolverDescription("Explicit middle point", 2, false);

        public IEnumerable<DynamicSystemState> Solve(IExplicitOrdinaryDifferentialEquationSystem equationSystem, IIndependentVariableStepStrategyFactory stepStrategyFactory)
        {
            if (equationSystem == null) throw new ArgumentNullException(nameof(equationSystem));
            if (stepStrategyFactory == null) throw new ArgumentNullException(nameof(stepStrategyFactory));

            var functions = equationSystem.ExecutableFunctions;
            var stepStrategy = stepStrategyFactory.Create(equationSystem.InitialState.IndependentVariable);

            var lastState = equationSystem.InitialState;
            while (true)
            {
                var step = stepStrategy.MoveNext();

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