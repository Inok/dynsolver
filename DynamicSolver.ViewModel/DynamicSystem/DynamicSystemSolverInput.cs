using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.DynamicSystem;
using DynamicSolver.Minimizer;
using DynamicSolver.ViewModel.Annotations;

namespace DynamicSolver.ViewModel.DynamicSystem
{
    public class DynamicSystemSolverInput
    {
        public ExplicitOrdinaryDifferentialEquationSystem System { get; }
        public double Step { get; set; }
        public double ModellingLimit { get; set; }

        public IReadOnlyCollection<VariableValue> Variables { get; }

        public DynamicSystemSolverInput([NotNull] ExplicitOrdinaryDifferentialEquationSystem system, [NotNull] IEnumerable<VariableValue> variables, double step, double modellingLimit)
        {
            if (system == null) throw new ArgumentNullException(nameof(system));
            if (variables == null) throw new ArgumentNullException(nameof(variables));

            System = system;
            Step = step;
            ModellingLimit = modellingLimit;
            Variables = variables.ToList();

            if (Variables.Any(v => !v.Value.HasValue))
            {
                throw new ArgumentException("Any variables has no value.");
            }
        }


    }
}