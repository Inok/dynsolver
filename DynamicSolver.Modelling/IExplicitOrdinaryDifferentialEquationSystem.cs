using System;
using System.Collections.Generic;
using DynamicSolver.Modelling.Solvers;

namespace DynamicSolver.Modelling
{
    public interface IExplicitOrdinaryDifferentialEquationSystem
    {
        IReadOnlyList<ExecutableFunctionInfo> ExecutableFunctions { get; }

        Dictionary<Tuple<string, string>, ExecutableFunctionInfo> Jacobian { get; }
    }
}