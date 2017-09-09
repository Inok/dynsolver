using System;
using System.Collections.Generic;
using DynamicSolver.DynamicSystem.Solvers;

namespace DynamicSolver.DynamicSystem
{
    public interface IExplicitOrdinaryDifferentialEquationSystem
    {
        IReadOnlyList<ExecutableFunctionInfo> ExecutableFunctions { get; }

        Dictionary<Tuple<string, string>, ExecutableFunctionInfo> Jacobian { get; }
    }
}