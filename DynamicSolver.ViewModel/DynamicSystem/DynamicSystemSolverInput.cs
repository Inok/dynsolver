using System;
using JetBrains.Annotations;

namespace DynamicSolver.ViewModel.DynamicSystem
{
    public class DynamicSystemSolverInput
    {
        public ExplicitOrdinaryDifferentialEquationSystemDefinition System { get; }

        public double Step { get; }
        public double Time { get; }

        public DynamicSystemSolverInput([NotNull] ExplicitOrdinaryDifferentialEquationSystemDefinition system, double step, double time)
        {
            if (system == null) throw new ArgumentNullException(nameof(system));
            if (step <= 0) throw new ArgumentOutOfRangeException(nameof(step));
            if (time <= 0) throw new ArgumentOutOfRangeException(nameof(time));

            System = system;
            Step = step;
            Time = time;
        }
    }
}