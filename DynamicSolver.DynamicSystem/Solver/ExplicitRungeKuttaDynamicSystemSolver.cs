using System.Collections.Generic;
using System.Linq;
using DynamicSolver.Expressions.Execution;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem.Solver
{
    public class ExplicitRungeKuttaDynamicSystemSolver : DynamicSystemSolver
    {
        public ExplicitRungeKuttaDynamicSystemSolver([NotNull] IExecutableFunctionFactory functionFactory, [NotNull] ExplicitOrdinaryDifferentialEquationSystem equationSystem)
            : base(functionFactory, equationSystem)
        {
        }

        protected override double CalculateNextFunctionValue(ExecutableFunctionInfo function, IDictionary<string, double> variables, double step)
        {
            var a1 = function.Function.OrderedArguments.Select(a => variables[a]).ToArray();
            var k1 = function.Function.Execute(a1);

            var a2 = function.Function.OrderedArguments.Select(a => variables[a] + k1 * step / 2).ToArray();
            var k2 = function.Function.Execute(a2);

            var a3 = function.Function.OrderedArguments.Select(a => variables[a] + k2 * step / 2).ToArray();
            var k3 = function.Function.Execute(a3);

            var a4 = function.Function.OrderedArguments.Select(a => variables[a] + k3 * step).ToArray();
            var k4 = function.Function.Execute(a4);

            return variables[function.Name] + step/6 * (k1 + 2*k2 + 2*k3 + k4);
        }
    }
}