using System;
using DynamicSolver.CoreMath.Syntax;
using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Execution.Interpreter
{
    public class InterpretedFunctionFactory : IExecutableFunctionFactory
    {
        public IExecutableFunction Create([NotNull] ISyntaxExpression statement)
        {
            if (statement == null) throw new ArgumentNullException(nameof(statement));

            return new InterpretedFunction(statement);
        }
    }
}