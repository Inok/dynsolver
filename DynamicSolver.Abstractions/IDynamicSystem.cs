using System.Collections.Generic;

namespace DynamicSolver.Abstractions
{
    public interface IDynamicSystem
    {
        ICollection<IExecutableFunction> Functions { get; set; }
    }
}