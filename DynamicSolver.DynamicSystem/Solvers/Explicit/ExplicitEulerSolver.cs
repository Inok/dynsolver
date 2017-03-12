using System;
using System.Collections.Generic;
using DynamicSolver.DynamicSystem.Step;

namespace DynamicSolver.DynamicSystem.Solvers.Explicit
{
    public class ExplicitEulerSolver : IDynamicSystemSolver
    {
        public DynamicSystemSolverDescription Description { get; } = new DynamicSystemSolverDescription("Euler", 1, false);

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