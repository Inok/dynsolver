﻿using System.Collections.Generic;

namespace DynamicSolver.Abstractions
{
    public interface IExpressionAnalyzer
    {
        IReadOnlyCollection<string> Variables { get; }
        ISet<string> GetVariablesSet();
    }
}