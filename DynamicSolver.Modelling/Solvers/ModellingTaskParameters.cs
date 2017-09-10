using System;

namespace DynamicSolver.Modelling.Solvers
{
    public class ModellingTaskParameters
    {
        public double Step { get; }

        public ModellingTaskParameters(double step)
        {
            if (step <= 0) throw new ArgumentOutOfRangeException(nameof(step));
            
            Step = step;
        }
    }
}