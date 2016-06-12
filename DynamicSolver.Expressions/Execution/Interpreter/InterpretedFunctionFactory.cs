using System;
using DynamicSolver.Expressions.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.Expressions.Execution.Interpreter
{
    public class InterpretedFunctionFactory : IExecutableFunctionFactory
    {
        public IExecutableFunction Create([NotNull] IStatement statement)
        {
            if (statement == null) throw new ArgumentNullException(nameof(statement));

            return new InterpretedFunction(statement);
        }
    }
}