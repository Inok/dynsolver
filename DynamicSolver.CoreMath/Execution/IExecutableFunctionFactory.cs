using DynamicSolver.CoreMath.Syntax;
using DynamicSolver.CoreMath.Syntax.Model;

namespace DynamicSolver.CoreMath.Execution
{
    public interface IExecutableFunctionFactory
    {
        IExecutableFunction Create(ISyntaxExpression statement);
    }
}