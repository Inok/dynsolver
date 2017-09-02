using System.Collections.Generic;
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
            [NotNull] DynamicSystemState initialState,
            [NotNull] ModellingTaskParameters parameters
        );
    }
}