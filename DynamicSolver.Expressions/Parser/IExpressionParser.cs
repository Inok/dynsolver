﻿using DynamicSolver.Abstractions.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.Expressions.Parser
{
    public interface IExpressionParser
    {
        [NotNull]
        IStatement Parse(string inputExpression);
    }
}