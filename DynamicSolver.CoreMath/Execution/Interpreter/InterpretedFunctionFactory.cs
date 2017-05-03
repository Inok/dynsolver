using System;
using DynamicSolver.CoreMath.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Execution.Interpreter
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