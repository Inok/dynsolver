using System.Collections.Generic;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem.Solver
{
    public interface IDynamicSystemSolver
    {
        IEnumerable<IDictionary<string, double>> Solve([NotNull] ExplicitOrdinaryDifferentialEquationSystem equationSystem, [NotNull] IDictionary<string, double> initialConditions, double step);
    }
}