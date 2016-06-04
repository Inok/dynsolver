using DynamicSolver.Expressions.Execution;

namespace DynamicSolver.DynamicSystem.Solver
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
    }
}