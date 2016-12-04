using System.Collections.Generic;
using System.Linq;
using DynamicSolver.Expressions.Execution;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem.Solver
{
    public class ExplicitMiddlePointDynamicSystemSolver : DynamicSystemSolver<object>
    {
        public ExplicitMiddlePointDynamicSystemSolver([NotNull] IExecutableFunctionFactory functionFactory) : base(functionFactory)
        {

        }

        protected override IReadOnlyDictionary<string, double> GetNextValues(IList<ExecutableFunctionInfo> functions, IReadOnlyDictionary<string, double> variables, double step, object extra)
        {
            var middleVars = variables.ToDictionary(v => v.Key, v => v.Value);

            foreach (var function in functions)
            {
                var arguments = function.Function.OrderedArguments.Select(a => variables[a]).ToArray();
                middleVars[function.Name] = variables[function.Name] + 0.5 * step * function.Function.Execute(arguments);
            }

            var vars = variables.ToDictionary(v => v.Key, v => v.Value);
            foreach (var function in functions)
            {
                var arguments = function.Function.OrderedArguments.Select(a => middleVars[a]).ToArray();
                vars[function.Name] = variables[function.Name] + step * function.Function.Execute(arguments);
            }

            return vars;
        }

        public override string ToString()
        {
            return $"Explicit middle point";
        }
    }
}