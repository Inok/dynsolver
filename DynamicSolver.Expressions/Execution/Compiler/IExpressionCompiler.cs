using DynamicSolver.Abstractions;

namespace DynamicSolver.Expressions.Execution.Compiler
{
    public interface IExpressionCompiler
    {
        IExecutableFunction Compile(string expression, string[] allowedArguments);
    }
}