using System.Collections.Generic;

namespace DynamicSolver.DynamicSystem
{
    public interface IDynamicSystemSolver
    {
        IEnumerable<IDictionary<string, double>> Solve(IDictionary<string, double> initialConditions, double step);
    }
}