using System;
using System.Collections.Generic;
using DynamicSolver.DynamicSystem;
using JetBrains.Annotations;

namespace DynamicSolver.ViewModel.DynamicSystem
{
    public class DynamicSystemSolverInput
    {
        public ExplicitOrdinaryDifferentialEquationSystem System { get; }
        public double Step { get; }
        public double ModellingLimit { get; }

        public IReadOnlyDictionary<string, double> Variables { get; }

        public DynamicSystemSolverInput([NotNull] ExplicitOrdinaryDifferentialEquationSystem system, [NotNull] IReadOnlyDictionary<string, double> variables, double step, double modellingLimit)
        {
            if (system == null) throw new ArgumentNullException(nameof(system));
            if (variables == null) throw new ArgumentNullException(nameof(variables));

            System = system;
            Step = step;
            ModellingLimit = modellingLimit;
            Variables = variables;            
        }
    }
}