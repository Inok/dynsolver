using System;
using System.Collections.Generic;
using DynamicSolver.DynamicSystem.Step;

namespace DynamicSolver.DynamicSystem.Solvers.SemiImplicit
{
    public class KDFastImplicitDynamicSystemSolver : IDynamicSystemSolver
    {
        public DynamicSystemSolverDescription Description { get; } = new DynamicSystemSolverDescription("KD (fast)", 2, true);

        public IEnumerable<DynamicSystemState> Solve(IExplicitOrdinaryDifferentialEquationSystem equationSystem, ModellingTaskParameters parameters)
        {
            if (equationSystem == null) throw new ArgumentNullException(nameof(equationSystem));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            var functions = equationSystem.ExecutableFunctions;
            var stepper = new FixedStepStepper(parameters.Step, equationSystem.InitialState.IndependentVariable);

            var tmpArgs = new Dictionary<string, double>(StringComparer.Ordinal);

            var firstHalfVars = new Dictionary<string, double>(StringComparer.Ordinal);

            var lastState = equationSystem.InitialState;
            while (true)
            {
                var step = stepper.MoveNext();

                var halfStep = step.Delta / 2;
                
                for (int i = 0; i < functions.Count; i++)
                {
                    var function = functions[i];

                    var startValue = lastState.DependentVariables[function.Name];
                    FillFirstHalfStepArguments(tmpArgs, functions, i-1, lastState.DependentVariables, firstHalfVars);
                    firstHalfVars[function.Name] = startValue + halfStep / 4 * function.Function.Execute(tmpArgs);

                    FillFirstHalfStepArguments(tmpArgs, functions, i, lastState.DependentVariables, firstHalfVars);
                    firstHalfVars[function.Name] = startValue + halfStep / 2 * (function.Function.Execute(tmpArgs));
                    
                    FillFirstHalfStepArguments(tmpArgs, functions, i, lastState.DependentVariables, firstHalfVars);
                    firstHalfVars[function.Name] = startValue + halfStep * function.Function.Execute(tmpArgs);
                }

                var secondHalfVars = new Dictionary<string, double>(StringComparer.Ordinal);
                
                for (int i = functions.Count - 1; i >= 0; i--)
                {
                    var function = functions[i];

                    var startValue = firstHalfVars[function.Name];
                    FillSecondHalfStepArguments(tmpArgs, functions, i+1, firstHalfVars, secondHalfVars);
                    secondHalfVars[function.Name] = startValue + halfStep / 4 * function.Function.Execute(tmpArgs);

                    FillSecondHalfStepArguments(tmpArgs, functions, i, firstHalfVars, secondHalfVars);
                    secondHalfVars[function.Name] = startValue + halfStep / 2 * (function.Function.Execute(tmpArgs));
                    
                    FillSecondHalfStepArguments(tmpArgs, functions, i, firstHalfVars, secondHalfVars);
                    secondHalfVars[function.Name] = startValue + halfStep * function.Function.Execute(tmpArgs);
                }

                yield return lastState = new DynamicSystemState(step.AbsoluteValue, secondHalfVars);
            }
        }

        private static void FillFirstHalfStepArguments(
            IDictionary<string, double> arguments,
            IReadOnlyList<ExecutableFunctionInfo> functions,
            int useNextUpToFunctionIndex,
            IReadOnlyDictionary<string, double> original,
            IReadOnlyDictionary<string, double> next)
        {
            for (var i = 0; i < functions.Count; i++)
            {
                var name = functions[i].Name;
                arguments[name] = i <= useNextUpToFunctionIndex ? next[name] : original[name];
            }
        }

        private static void FillSecondHalfStepArguments(
            IDictionary<string, double> arguments,
            IReadOnlyList<ExecutableFunctionInfo> functions,
            int useNextFromFunctionIndex,
            IReadOnlyDictionary<string, double> original,
            IReadOnlyDictionary<string, double> next)
        {
            for (var i = 0; i < functions.Count; i++)
            {
                var name = functions[i].Name;
                arguments[name] = i >= useNextFromFunctionIndex ? next[name] : original[name];
            }
        }
    }
}