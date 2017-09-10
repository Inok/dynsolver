using System;
using DynamicSolver.Modelling.Solvers;
using JetBrains.Annotations;

namespace DynamicSolver.ViewModel.DynamicSystem
{
    public class ModellingSettings
    {
        public IDynamicSystemSolver Solver { get; }

        public double Step { get; }

        public double Time { get; }

        public ModellingSettings([NotNull] IDynamicSystemSolver solver, double step, double time)
        {
            if (solver == null) throw new ArgumentNullException(nameof(solver));
            if (step <= 0) throw new ArgumentOutOfRangeException(nameof(step));
            if (time <= 0) throw new ArgumentOutOfRangeException(nameof(time));

            Solver = solver;
            Step = step;
            Time = time;
        }
    }
}