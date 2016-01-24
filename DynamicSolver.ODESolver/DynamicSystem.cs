using System;
using DynamicSolver.Abstractions;
using DynamicSolver.Abstractions.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.ODESolver
{
    public class DynamicSystem
    {
        public IEquation[] Equations { get; }

        public IInitialState[] Initial { get; }

        public Range<double> TimeRange { get; }

        public double StepIncrement { get; }

        public DynamicSystem([NotNull] IEquation[] equations, [NotNull] IInitialState[] initial, [NotNull] Range<double> timeRange, double stepIncrement)
        {
            if (equations == null) throw new ArgumentNullException(nameof(equations));
            if (initial == null) throw new ArgumentNullException(nameof(initial));
            if (timeRange == null) throw new ArgumentNullException(nameof(timeRange));
            if (stepIncrement <= 0) throw new ArgumentOutOfRangeException(nameof(stepIncrement));

            Equations = equations;
            Initial = initial;
            TimeRange = timeRange;
            StepIncrement = stepIncrement;
        }
    }
}