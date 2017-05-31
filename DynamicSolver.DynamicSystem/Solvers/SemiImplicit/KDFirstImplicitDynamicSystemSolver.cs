using System;
using System.Collections.Generic;
using DynamicSolver.DynamicSystem.Step;

namespace DynamicSolver.DynamicSystem.Solvers.SemiImplicit
{
    public class KDFirstImplicitDynamicSystemSolver : IDynamicSystemSolver
    {
        public DynamicSystemSolverDescription Description { get; } = new DynamicSystemSolverDescription("KD (I,E)", 2, true);

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
                
                for (int i = functions.Count - 1; i >= 0; i--)
                {
                    var function = functions[i];

                    var derivationFunctionKey = Tuple.Create(function.Name, function.Name);

                    var k = 0;
                    var previous = lastState.DependentVariables[function.Name];
                    do
                    {
                        FillImplicitFunctionArguments(tmpArgs, functions, i, lastState.DependentVariables, firstHalfVars);
                        tmpArgs[function.Name] = previous;

                        var numerator = previous - lastState.DependentVariables[function.Name] - halfStep * function.Execute(tmpArgs);
                        var denominator = 1 - halfStep * equationSystem.Jacobian[derivationFunctionKey].Execute(tmpArgs);
                        var newValue = previous - numerator / denominator;
                        firstHalfVars[function.Name] = newValue;

                        if (++k > 15 || Math.Abs(newValue - previous) <= step.Delta * 1e-12)
                        {
                            break;
                        }

                        previous = newValue;
                    } while (true);
                }

                var secondHalfVars = new Dictionary<string, double>(StringComparer.Ordinal);
                for (int i = 0; i < functions.Count; i++)
                {
                    var function = functions[i];

                    FillExplicitFunctionArguments(tmpArgs, functions, i, firstHalfVars, secondHalfVars);

                    var functionValue = function.Execute(tmpArgs);

                    secondHalfVars[function.Name] = firstHalfVars[function.Name] + halfStep * functionValue;
                }

                yield return lastState = new DynamicSystemState(step.AbsoluteValue, secondHalfVars);
            }
        }

        private static void FillExplicitFunctionArguments(IDictionary<string, double> arguments, IReadOnlyList<ExecutableFunctionInfo> functions, int firstItemsCountAsNextStepPoint, IReadOnlyDictionary<string, double> currentStepVariables, IReadOnlyDictionary<string, double> nextStepValues)
        {
            for (var i = 0; i < functions.Count; i++)
            {
                var name = functions[i].Name;
                arguments[name] = i < firstItemsCountAsNextStepPoint ? nextStepValues[name] : currentStepVariables[name];
            }
        }

        private static void FillImplicitFunctionArguments(IDictionary<string, double> arguments, IReadOnlyList<ExecutableFunctionInfo> functions, int implicitFunctionIndex, IReadOnlyDictionary<string, double> firstHalfVars, IReadOnlyDictionary<string, double> secondHalfVars)
        {
            for (var i = 0; i < functions.Count; i++)
            {
                var name = functions[i].Name;
                arguments[name] = i <= implicitFunctionIndex ? firstHalfVars[name] : secondHalfVars[name];
            }
        }
    }
}