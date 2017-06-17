using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.DynamicSystem;
using DynamicSolver.DynamicSystem.Solvers;
using JetBrains.Annotations;

namespace DynamicSolver.ViewModel.DynamicSystem
{
    public class ExplicitOrdinaryDifferentialEquationSystemDefinition
    {
        [NotNull] public IReadOnlyCollection<ExplicitOrdinaryDifferentialEquation> Equations { get; }
        [NotNull] public DynamicSystemState InitialState { get; }

        public ExplicitOrdinaryDifferentialEquationSystemDefinition(
            [NotNull] IEnumerable<ExplicitOrdinaryDifferentialEquation> equations,
            [NotNull] DynamicSystemState initialState)
        {
            if (equations == null) throw new ArgumentNullException(nameof(equations));
            if (initialState == null) throw new ArgumentNullException(nameof(initialState));

            Equations = equations.ToList();
            InitialState = initialState;
        }
    }
}