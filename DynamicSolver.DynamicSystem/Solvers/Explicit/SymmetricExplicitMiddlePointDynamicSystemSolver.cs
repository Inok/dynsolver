﻿using System;
using System.Collections.Generic;
using DynamicSolver.DynamicSystem.Step;

namespace DynamicSolver.DynamicSystem.Solvers.Explicit
{
    public class SymmetricExplicitMiddlePointDynamicSystemSolver : IDynamicSystemSolver
    {
        public DynamicSystemSolverDescription Description { get; } = new DynamicSystemSolverDescription("Symmetric explicit middle point", 2, true, true);

        public IEnumerable<DynamicSystemState> Solve(IExplicitOrdinaryDifferentialEquationSystem equationSystem, ModellingTaskParameters parameters)
        {
            if (equationSystem == null) throw new ArgumentNullException(nameof(equationSystem));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            var functions = equationSystem.ExecutableFunctions;
            var stepper = new FixedStepStepper(parameters.Step, equationSystem.InitialState.IndependentVariable);

            var previousState = equationSystem.InitialState.DependentVariables;
            var step = stepper.MoveNext();
            
            var middlePointState = new Dictionary<string, double>();
            foreach (var function in functions)
            {
                middlePointState[function.Name] = previousState[function.Name] + step.Delta * function.Execute(previousState);
            }
            
            yield return new DynamicSystemState(step.AbsoluteValue, middlePointState);
            
            while (true)
            {
                step = stepper.MoveNext();
                
                var dependentVariables = new Dictionary<string, double>();
                foreach (var function in functions)
                {
                    dependentVariables[function.Name] = previousState[function.Name] + 2*step.Delta * function.Execute(middlePointState);
                }

                yield return new DynamicSystemState(step.AbsoluteValue, dependentVariables);

                previousState = middlePointState;
                middlePointState = dependentVariables;
            }
        }
    }
}