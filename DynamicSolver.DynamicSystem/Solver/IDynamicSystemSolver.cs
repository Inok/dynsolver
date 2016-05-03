using System.Collections.Generic;

namespace DynamicSolver.DynamicSystem.Solver
{
    public interface IDynamicSystemSolver
    {
        IEnumerable<IDictionary<string, double>> Solve(IDictionary<string, double> initialConditions, double step);
    }
}