using DynamicSolver.CoreMath.Syntax;

namespace DynamicSolver.CoreMath.Execution
{
    public interface IExecutableFunctionFactory
    {
        IExecutableFunction Create(IExpression statement);
    }
}