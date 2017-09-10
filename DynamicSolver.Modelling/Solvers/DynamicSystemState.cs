using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace DynamicSolver.Modelling.Solvers
{
    public class DynamicSystemState
    {
        public double IndependentVariable { get; }

        public IReadOnlyDictionary<string, double> DependentVariables { get; }

        public DynamicSystemState(double independentVariable, [NotNull] IReadOnlyDictionary<string, double> dependentVariables)
        {
            if (dependentVariables == null) throw new ArgumentNullException(nameof(dependentVariables));
            IndependentVariable = independentVariable;
            DependentVariables = dependentVariables;
        }
    }
}