using System.Collections.Generic;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem.Solver
{
    public interface IDynamicSystemSolver
    {
        IEnumerable<IReadOnlyDictionary<string, double>> Solve([NotNull] ExplicitOrdinaryDifferentialEquationSystem equationSystem, [NotNull] IReadOnlyDictionary<string, double> initialConditions, double step);
    }
}