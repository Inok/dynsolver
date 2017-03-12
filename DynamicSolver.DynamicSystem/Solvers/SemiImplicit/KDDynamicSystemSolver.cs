using System;
using System.Collections.Generic;
using DynamicSolver.DynamicSystem.Step;

namespace DynamicSolver.DynamicSystem.Solvers.SemiImplicit
{
    public class KDDynamicSystemSolver : IDynamicSystemSolver
    {
        public DynamicSystemSolverDescription Description { get; } = new DynamicSystemSolverDescription("KD", 2, true);

        public IEnumerable<DynamicSystemState> Solve(IExplicitOrdinaryDifferentialEquationSystem equationSystem, IIndependentVariableStepStrategyFactory stepStrategyFactory)
        {
            if (equationSystem == null) throw new ArgumentNullException(nameof(equationSystem));
            if (stepStrategyFactory == null) throw new ArgumentNullException(nameof(stepStrategyFactory));

            var functions = equationSystem.ExecutableFunctions;
            var stepStrategy = stepStrategyFactory.Create(equationSystem.InitialState.IndependentVariable);

            var service = new JacobianCalculationService();

            var lastState = equationSystem.InitialState;
            while (true)
            {
                var step = stepStrategy.MoveNext();

                var halfStep = step.Delta / 2;

                var firstHalfVars = new Dictionary<string, double>();
                for (int i = 0; i < functions.Count; i++)
                {
                    var function = functions[i];

                    var arguments = GetExplicitFunctionArguments(functions, i, lastState.DependentVariables, firstHalfVars);

                    var functionValue = function.Execute(arguments);

                    firstHalfVars[function.Name] = lastState.DependentVariables[function.Name] + halfStep * functionValue;
                }

                var secondHalfVars = new Dictionary<string, double>();
                for (int i = functions.Count - 1; i >= 0; i--)
                {
                    var function = functions[i];

                    var derivationFunctionKey = Tuple.Create(function.Name, function.Name);

                    var k = 0;
                    var previous = firstHalfVars[function.Name];
                    do
                    {
                        var arguments = GetImplicitFunctionArguments(functions, i, firstHalfVars, secondHalfVars);
                        arguments[function.Name] = previous;

                        var numerator = previous - firstHalfVars[function.Name] - halfStep * function.Execute(arguments);
                        var denominator = 1 - halfStep * equationSystem.Jacobian[derivationFunctionKey].Execute(arguments);
                        var newValue = previous - numerator / denominator;
                        secondHalfVars[function.Name] = newValue;

                        if (++k > 5 || Math.Abs(newValue - previous) <= step.Delta * 10e-5)
                        {
                            break;
                        }

                        previous = newValue;

                    } while (true);
                }

                yield return lastState = new DynamicSystemState(step.AbsoluteValue, secondHalfVars);
            }
        }

        private static Dictionary<string, double> GetExplicitFunctionArguments(IReadOnlyList<ExecutableFunctionInfo> functions, int firstItemsCountAsNextStepPoint, IReadOnlyDictionary<string, double> currentStepVariables, IReadOnlyDictionary<string, double> nextStepValues)
        {
            var result = new Dictionary<string, double>();

            for (var i = 0; i < functions.Count; i++)
            {
                var name = functions[i].Name;
                result[name] = i < firstItemsCountAsNextStepPoint ? nextStepValues[name] : currentStepVariables[name];
            }

            return result;
        }

        private static Dictionary<string, double> GetImplicitFunctionArguments(IReadOnlyList<ExecutableFunctionInfo> functions, int implicitFunctionIndex, Dictionary<string, double> firstHalfVars, Dictionary<string, double> secondHalfVars)
        {
            var result = new Dictionary<string, double>();

            for (var i = 0; i < functions.Count; i++)
            {
                var name = functions[i].Name;
                result[name] = i <= implicitFunctionIndex ? firstHalfVars[name] : secondHalfVars[name];
            }

            return result;
        }
    }
}