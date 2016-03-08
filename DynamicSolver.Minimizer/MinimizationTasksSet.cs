using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace DynamicSolver.Minimizer
{
    public class MinimizationResultSet
    {
        public IReadOnlyCollection<MinimizationResult> Results { get; }

        public MinimizationResultSet([NotNull] IReadOnlyCollection<MinimizationResult> results)
        {
            if (results == null) throw new ArgumentNullException(nameof(results));

            Results = results;
        }
    }
}