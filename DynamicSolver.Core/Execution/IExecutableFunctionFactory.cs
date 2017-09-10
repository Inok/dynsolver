using DynamicSolver.Core.Syntax.Model;

namespace DynamicSolver.Core.Execution
{
    public interface IExecutableFunctionFactory
    {
        IExecutableFunction Create(ISyntaxExpression statement);
    }
}