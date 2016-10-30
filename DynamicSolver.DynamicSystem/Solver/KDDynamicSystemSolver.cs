using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.Expressions.Execution;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem.Solver
{
    public class KDDynamicSystemSolver : DynamicSystemSolver<Dictionary<string, Dictionary<string, IExecutableFunction>>>, IDynamicSystemSolver
    {
        public KDDynamicSystemSolver([NotNull] IExecutableFunctionFactory functionFactory) : base(functionFactory) 
        {            
        }

        protected override Dictionary<string, Dictionary<string, IExecutableFunction>> GetExtraArguments(ExplicitOrdinaryDifferentialEquationSystem equationSystem, IList<ExecutableFunctionInfo> functions, IReadOnlyDictionary<string, double> initialConditions, double step)
        {
            var service = new JacobianCalculationService();
            var nextStateExpressions = service.ExpressNextStateVariableValueExpressions(equationSystem);
            return nextStateExpressions.ToDictionary(p => p.Key, p => p.Value.ToDictionary(t => t.Key, t => FunctionFactory.Create(t.Value)));
        }

        protected override IReadOnlyDictionary<string, double> GetNextValues(IList<ExecutableFunctionInfo> functions, IReadOnlyDictionary<string, double> variables, double step, Dictionary<string, Dictionary<string, IExecutableFunction>> extra)
        {
            var halfStep = step/2;

            var firstHalfVars = new Dictionary<string, double>();
            for (int i = 0; i < functions.Count; i++)
            {
                var function = functions[i];

                var arguments = GetExplicitFunctionArguments(functions, i, variables, firstHalfVars);

                var functionValue = ExecuteFunction(function.Function, arguments);

                firstHalfVars[function.Name] = variables[function.Name] + halfStep * functionValue;
            }

            var secondHalfVars = new Dictionary<string, double>();
            for (int i = functions.Count - 1; i >= 0; i--)
            {
                var function = functions[i];

                var k = 0;
                var previous = firstHalfVars[function.Name];
                do
                {
                    var arguments = GetImplicitFunctionArguments(functions, i, firstHalfVars, secondHalfVars);
                    var derivationFunction = extra[function.Name][function.Name];
                    var exchangeValue = new KeyValuePair<string, double>(function.Name, previous);

                    var numerator = previous - firstHalfVars[function.Name] - halfStep * ExecuteFunction(function.Function, arguments, exchangeValue);
                    var denominator = 1 - halfStep * ExecuteFunction(derivationFunction, arguments, exchangeValue);
                    var newValue = previous - numerator / denominator;
                    secondHalfVars[function.Name] = newValue;

                    if (++k > 10 || Math.Abs(newValue - previous) <= step * 10e-7)
                    {
                        break;
                    }

                    previous = newValue;

                } while (true);
            }

            return secondHalfVars;
        }

        private double ExecuteFunction(IExecutableFunction function, IDictionary<string, double> arguments)
        {
            return function.Execute(function.OrderedArguments.Select(a => arguments[a]).ToArray());
        }

        private double ExecuteFunction(IExecutableFunction function, IDictionary<string, double> arguments, KeyValuePair<string, double> exchangeValue)
        {
            return function.Execute(function.OrderedArguments.Select(a => a == exchangeValue.Key ? exchangeValue.Value : arguments[a]).ToArray());
        }

        private static IDictionary<string, double> GetExplicitFunctionArguments(IList<ExecutableFunctionInfo> functions, int firstItemsCountAsNextStepPoint, IReadOnlyDictionary<string, double> currentStepVariables, IReadOnlyDictionary<string, double> nextStepValues)
        {
            var result = new Dictionary<string, double>();

            for (var i = 0; i < functions.Count; i++)
            {
                var name = functions[i].Name;
                result[name] = i < firstItemsCountAsNextStepPoint ? nextStepValues[name] : currentStepVariables[name];
            }

            return result;
        }

        private IDictionary<string, double> GetImplicitFunctionArguments(IList<ExecutableFunctionInfo> functions, int implicitFunctionIndex, Dictionary<string, double> firstHalfVars, Dictionary<string, double> secondHalfVars)
        {
            var result = new Dictionary<string, double>();

            for (var i = 0; i < functions.Count; i++)
            {
                var name = functions[i].Name;
                result[name] = i <= implicitFunctionIndex ? firstHalfVars[name] : secondHalfVars[name];
            }

            return result;
        }

        public override string ToString()
        {
            return $"KD";
        }

    }
}