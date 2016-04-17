using System;
using DynamicSolver.Abstractions;
using DynamicSolver.Abstractions.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.Expressions.Execution.Compiler
{
    public class CompiledFunctionFactory : IExecutableFunctionFactory
    {
        public IExecutableFunction Create([NotNull] IStatement statement)
        {
            if (statement == null) throw new ArgumentNullException(nameof(statement));

            return new CompiledFunction(statement);
        }
    }
}