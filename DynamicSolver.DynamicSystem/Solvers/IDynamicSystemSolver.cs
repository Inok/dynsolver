using System.Collections.Generic;
using DynamicSolver.DynamicSystem.Step;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem.Solvers
{
    public interface IDynamicSystemSolver
    {
        [NotNull]
        DynamicSystemSolverDescription Description { get; }

        [NotNull, ItemNotNull]
        IEnumerable<DynamicSystemState> Solve(
            [NotNull] IExplicitOrdinaryDifferentialEquationSystem equationSystem,
            [NotNull] IIndependentVariableStepStrategy stepStrategy
        );
    }
}