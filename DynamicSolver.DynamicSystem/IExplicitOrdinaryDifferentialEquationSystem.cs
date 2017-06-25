﻿using System;
using System.Collections.Generic;
using DynamicSolver.CoreMath.Execution;
using DynamicSolver.DynamicSystem.Solvers;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem
{
    public interface IExplicitOrdinaryDifferentialEquationSystem
    {
        IExecutableFunctionFactory FunctionFactory { get; }

        IReadOnlyCollection<ExplicitOrdinaryDifferentialEquation> Equations { get; }

        DynamicSystemState InitialState { get; }

        IReadOnlyList<ExecutableFunctionInfo> ExecutableFunctions { get; }

        Dictionary<Tuple<string, string>, ExecutableFunctionInfo> Jacobian { get; }

        IExplicitOrdinaryDifferentialEquationSystem WithInitialState([NotNull] DynamicSystemState state);
    }
}