using System;
using System.Collections.Generic;
using DynamicSolver.DynamicSystem.Step;

namespace DynamicSolver.DynamicSystem.Solvers.SemiImplicit
{
    public class KDFirstExplicitDynamicSystemSolver : IDynamicSystemSolver
    {
        public DynamicSystemSolverDescription Description { get; } = new DynamicSystemSolverDescription("KD (E,I)", 2, true);

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

                    FillExplicitFunctionArguments(tmpArgs, functions, i, lastState.DependentVariables, firstHalfVars);

                    var functionValue = function.Execute(tmpArgs);

                    firstHalfVars[function.Name] = lastState.DependentVariables[function.Name] + halfStep * functionValue;
                }

                var secondHalfVars = new Dictionary<string, double>(StringComparer.Ordinal);
                for (int i = functions.Count - 1; i >= 0; i--)
                {
                    var function = functions[i];

                    var derivationFunctionKey = Tuple.Create(function.Name, function.Name);

                    var k = 0;
                    var previous = firstHalfVars[function.Name];
                    do
                    {
                        FillImplicitFunctionArguments(tmpArgs, functions, i, firstHalfVars, secondHalfVars);
                        tmpArgs[function.Name] = previous;

                        var numerator = previous - firstHalfVars[function.Name] - halfStep * function.Execute(tmpArgs);
                        var denominator = 1 - halfStep * equationSystem.Jacobian[derivationFunctionKey].Execute(tmpArgs);
                        var newValue = previous - numerator / denominator;
                        secondHalfVars[function.Name] = newValue;

                        if (++k > 15 || Math.Abs(newValue - previous) <= step.Delta * 1e-12)
                        {
                            break;
                        }

                        previous = newValue;
                    } while (true);
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

        private static void FillImplicitFunctionArguments(IDictionary<string, double> arguments, IReadOnlyList<ExecutableFunctionInfo> functions, int implicitFunctionIndex, Dictionary<string, double> firstHalfVars, Dictionary<string, double> secondHalfVars)
        {
            for (var i = 0; i < functions.Count; i++)
            {
                var name = functions[i].Name;
                arguments[name] = i <= implicitFunctionIndex ? firstHalfVars[name] : secondHalfVars[name];
            }
        }
    }
}