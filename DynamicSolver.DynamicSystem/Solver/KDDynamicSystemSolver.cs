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
                var arguments = function.Function.OrderedArguments.Select(a =>
                    functions.Select(f => f.Name).Take(i + 1).Contains(a)
                        ? (firstHalfVars.ContainsKey(a) ? firstHalfVars[a] : extra[a].Execute(extra[a].OrderedArguments.Select(t => t == "h" ? halfStep : variables[t]).ToArray()))
                        : variables[a]).ToArray();

                firstHalfVars[function.Name] = variables[function.Name] + halfStep * function.Function.Execute(arguments);
            }

            var secondHalfVars = new Dictionary<string, double>();
            for (int i = functions.Count - 1; i >= 0; i--)
            {
                var function = functions[i];
                var arguments = function.Function.OrderedArguments.Select(a =>
                    functions.Select(f => f.Name).Skip(i + 1).Contains(a)
                        ? (secondHalfVars.ContainsKey(a) ? secondHalfVars[a] : extra[a].Execute(extra[a].OrderedArguments.Select(t => t == "h" ? halfStep : firstHalfVars[t]).ToArray()))
                        : firstHalfVars[a]).ToArray();

                secondHalfVars[function.Name] = firstHalfVars[function.Name] + halfStep * function.Function.Execute(arguments);
            }

            return secondHalfVars;
        }

        public override string ToString()
        {
            return $"KD";
        }

    }
}