using System;
using DynamicSolver.Core.Syntax.Model;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Execution.Interpreter
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