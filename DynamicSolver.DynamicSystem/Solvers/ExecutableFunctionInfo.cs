using System.Collections.Generic;
using DynamicSolver.Core.Execution;

namespace DynamicSolver.DynamicSystem.Solvers
{
    public struct ExecutableFunctionInfo
    {
        public string Name { get; }
        public IExecutableFunction Function { get; }

        public ExecutableFunctionInfo(string name, IExecutableFunction function)
        {
            Name = name;
            Function = function;
        }

        public double Execute(IReadOnlyDictionary<string, double> variables)
        {
            var arguments = GetArgumentsArray(variables);
            return Function.Execute(arguments);
        }

        public double[] GetArgumentsArray(IReadOnlyDictionary<string, double> variables)
        {
            var args = Function.OrderedArguments;

            var arguments = new double[args.Count];

            var i = 0;
            foreach (var arg in args)
            {
                arguments[i++] = variables[arg];
            }

            return arguments;
        }
    }
}