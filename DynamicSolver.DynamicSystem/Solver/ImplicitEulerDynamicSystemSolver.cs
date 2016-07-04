using System.Collections.Generic;
using System.Linq;
using DynamicSolver.Expressions.Execution;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem.Solver
{
    public class ImplicitEulerDynamicSystemSolver : DynamicSystemSolver<Dictionary<string, IExecutableFunction>>
    {
        public ImplicitEulerDynamicSystemSolver([NotNull] IExecutableFunctionFactory functionFactory) : base(functionFactory)
        {
            
        }

        protected override IReadOnlyDictionary<string, double> GetNextValues(IList<ExecutableFunctionInfo> functions, IReadOnlyDictionary<string, double> variables, double step, Dictionary<string, IExecutableFunction> extra)
        {
            var vars = variables.ToDictionary(v => v.Key, v => v.Value);

            foreach (var function in functions)
            {
                var nextVariables = function.Function.OrderedArguments.ToDictionary(
                    name => name,
                    name =>
                    {
                        if (function.Name != name)
                        {
                            return variables[name];
                        }

                        var nextStepFunction = extra[function.Name];
                        return nextStepFunction.Execute(nextStepFunction.OrderedArguments.Select(a => a == "h" ? step : variables[a]).ToArray());
                    });
                
                vars[function.Name] = variables[function.Name] + step*function.Function.Execute(nextVariables);
            }

            return vars;
        }

        protected override Dictionary<string, IExecutableFunction> GetExtraArguments(ExplicitOrdinaryDifferentialEquationSystem equationSystem, IList<ExecutableFunctionInfo> functions, IReadOnlyDictionary<string, double> initialConditions, double step)
        {
            var service = new NextStateVariableValueCalculationService();
            return service.ExpressNextStateVariableValueExpressions(equationSystem, "h").ToDictionary(p => p.Key, p => FunctionFactory.Create(p.Value));
        }

        public override string ToString()
        {
            return "Implicit Euler";
        }
    }
}