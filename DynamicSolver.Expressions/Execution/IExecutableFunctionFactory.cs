using DynamicSolver.Abstractions;
using DynamicSolver.Abstractions.Expression;

namespace DynamicSolver.Expressions.Execution
{
    public interface IExecutableFunctionFactory
    {
        IExecutableFunction Create(IStatement statement);
    }
}