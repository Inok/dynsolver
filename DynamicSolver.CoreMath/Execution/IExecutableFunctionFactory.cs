using DynamicSolver.CoreMath.Expression;

namespace DynamicSolver.CoreMath.Execution
{
    public interface IExecutableFunctionFactory
    {
        IExecutableFunction Create(IStatement statement);
    }
}