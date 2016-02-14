using DynamicSolver.Abstractions;

namespace DynamicSolver.ExpressionCompiler
{
    public interface IExpressionCompiler
    {
        IExecutableFunction Compile(string expression, string[] allowedArguments);
    }
}