using System.Collections.Generic;
using System.Linq;
using DynamicSolver.Expressions.Execution;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem.Solver
{
    public class KDDynamicSystemSolver : DynamicSystemSolver<Dictionary<string, IExecutableFunction>>, IDynamicSystemSolver
    {
        public KDDynamicSystemSolver([NotNull] IExecutableFunctionFactory functionFactory) : base(functionFactory) 
        {            
        }

        protected override Dictionary<string, IExecutableFunction> GetExtraArguments(ExplicitOrdinaryDifferentialEquationSystem equationSystem, IList<ExecutableFunctionInfo> functions, IReadOnlyDictionary<string, double> initialConditions, double step)
        {
            var service = new NextStateVariableValueCalculationService();
            var nextStateExpressions = service.ExpressNextStateVariableValueExpressions(equationSystem, "h");
            return nextStateExpressions.ToDictionary(p => p.Key, p => FunctionFactory.Create(p.Value));
        }

        protected override IReadOnlyDictionary<string, double> GetNextValues(IList<ExecutableFunctionInfo> functions, IReadOnlyDictionary<string, double> variables, double step, Dictionary<string, IExecutableFunction> extra)
        {
            var halfStep = step/2;

            var firstHalfVars = new Dictionary<string, double>();

            for (int i = 0; i < functions.Count; i++)
            {
                var function = functions[i];

                var newtonBasedArguments = functions.Select(f => f.Name).Take(i + 1).ToList();
                var arguments = GetFunctionArguments(function, extra, newtonBasedArguments, variables, firstHalfVars, halfStep);

                firstHalfVars[function.Name] = variables[function.Name] + halfStep * function.Function.Execute(arguments.ToArray());
            }

            var secondHalfVars = new Dictionary<string, double>();
            for (int i = functions.Count - 1; i >= 0; i--)
            {
                var function = functions[i];

                var newtonBasedArguments = functions.Select(f => f.Name).Skip(i + 1).ToList();
                var arguments = GetFunctionArguments(function, extra, newtonBasedArguments, firstHalfVars, secondHalfVars, halfStep);

                secondHalfVars[function.Name] = firstHalfVars[function.Name] + halfStep * function.Function.Execute(arguments.ToArray());
            }

            return secondHalfVars;
        }

        private static IEnumerable<double> GetFunctionArguments(
            ExecutableFunctionInfo function,
            IReadOnlyDictionary<string, IExecutableFunction> nextStateExpressions,
            ICollection<string> newtonBasedArguments,
            IReadOnlyDictionary<string, double> currentStepVariables,
            IReadOnlyDictionary<string, double> nextStepValues,
            double step)
        {
            foreach (var arg in function.Function.OrderedArguments)
            {
                if (newtonBasedArguments.Contains(arg))
                {
                    if (nextStepValues.ContainsKey(arg))
                    {
                        yield return nextStepValues[arg];
                    }
                    else
                    {
                        var arguments = nextStateExpressions[arg].OrderedArguments.Select(a => a == "h" ? step : currentStepVariables[a]).ToArray();
                        yield return nextStateExpressions[arg].Execute(arguments);
                    }
                }
                else
                {
                    yield return currentStepVariables[arg];
                }
            }
        }

        public override string ToString()
        {
            return $"KD";
        }

    }
}