using DynamicSolver.Abstractions;

namespace DynamicSolver.ExpressionCompiler
{
    public interface IExpressionCompiler
    {
        IFunction Compile(string expression, string[] allowedArguments);
    }
}