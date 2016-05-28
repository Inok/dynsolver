using DynamicSolver.Expressions.Expression;

namespace DynamicSolver.Expressions.Execution
{
    public interface IExecutableFunctionFactory
    {
        IExecutableFunction Create(IStatement statement);
    }
}