using System.Collections.Generic;

namespace DynamicSolver.Abstractions
{
    public interface IDynamicSystem
    {
        ICollection<IFunction> Functions { get; set; }
    }
}