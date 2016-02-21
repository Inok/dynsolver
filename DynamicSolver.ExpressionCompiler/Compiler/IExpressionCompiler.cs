﻿using DynamicSolver.Abstractions;

namespace DynamicSolver.ExpressionCompiler.Compiler
{
    public interface IExpressionCompiler
    {
        IExecutableFunction Compile(string expression, string[] allowedArguments);
    }
}