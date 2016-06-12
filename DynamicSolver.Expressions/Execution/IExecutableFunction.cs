﻿using System.Collections.Generic;
using JetBrains.Annotations;

namespace DynamicSolver.Expressions.Execution
{
    public interface IExecutableFunction
    {
        [NotNull]
        IReadOnlyCollection<string> OrderedArguments { get; } 
        
        double Execute([NotNull] double[] arguments);
        double Execute([NotNull] IReadOnlyDictionary<string, double> arguments);
    }
}