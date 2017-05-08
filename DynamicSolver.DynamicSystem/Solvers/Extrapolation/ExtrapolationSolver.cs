using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.DynamicSystem.Step;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem.Solvers.Extrapolation
{
    public class ExtrapolationSolver : IDynamicSystemSolver
    {
        public DynamicSystemSolverDescription Description { get; }

        [NotNull]
        protected IDynamicSystemSolver BaseSolver { get; }

        protected int ExtrapolationStages { get; }

        public ExtrapolationSolver([NotNull] IDynamicSystemSolver baseSolver, int extrapolationStages)
        {
            if (baseSolver == null) throw new ArgumentNullException(nameof(baseSolver));
            if (extrapolationStages <= 0) throw new ArgumentOutOfRangeException(nameof(extrapolationStages));

            BaseSolver = baseSolver;
            ExtrapolationStages = extrapolationStages;

            var name = $"Extrapolation method ({extrapolationStages}-staged, based on {baseSolver.Description.Name})";
            var order = baseSolver.Description.Order + (extrapolationStages - 1) * (baseSolver.Description.IsSymmetric ? 2 : 1);
            Description = new DynamicSystemSolverDescription(name, order, false);
        }

        public IEnumerable<DynamicSystemState> Solve(IExplicitOrdinaryDifferentialEquationSystem equationSystem, IIndependentVariableStepStrategy stepStrategy)
        {
            if (equationSystem == null) throw new ArgumentNullException(nameof(equationSystem));
            if (stepStrategy == null) throw new ArgumentNullException(nameof(stepStrategy));

            var stepper = stepStrategy.Create(equationSystem.InitialState.IndependentVariable);

            var extrapolationCoefficients = GetExtrapolationCoefficients(ExtrapolationStages);
            var buffer = new double[ExtrapolationStages, ExtrapolationStages];
            var solvesBuffer = new IReadOnlyDictionary<string, double>[ExtrapolationStages];

            var lastState = equationSystem.InitialState;
            while (true)
            {
                var state = equationSystem.WithInitialState(lastState);

                var step = stepper.MoveNext();

                for (var i = 0; i < solvesBuffer.Length; i++)
                {
                    solvesBuffer[i] = MakeExtrapolationSteps(state, step, extrapolationCoefficients[i]).DependentVariables;
                }

                var newValues = new Dictionary<string, double>();

                foreach (var variable in lastState.DependentVariables.Keys)
                {
                    for (var j = 0; j < ExtrapolationStages; j++)
                    {
                        buffer[j, 0] = solvesBuffer[j][variable];

                        for (var k = 0; k < j; k++)
                        {
                            buffer[j, k + 1] = buffer[j, k] + (buffer[j, k] - buffer[j - 1, k]) / (Math.Pow((double)extrapolationCoefficients[j] / extrapolationCoefficients[j - k - 1], BaseSolver.Description.Order) - 1);
                        }
                    }

                    newValues[variable] = buffer[ExtrapolationStages - 1, ExtrapolationStages - 1];
                }

                yield return lastState = new DynamicSystemState(step.AbsoluteValue, newValues);
            }
        }

        protected virtual DynamicSystemState MakeExtrapolationSteps(
            IExplicitOrdinaryDifferentialEquationSystem state,
            IndependentVariableStep step,
            int extrapolationCoefficient
            )
        {
            var stepSize = step.Delta / extrapolationCoefficient;
            var extrapolationStepsLastValue = BaseSolver.Solve(state, new FixedStepStrategy(stepSize)).Take(extrapolationCoefficient).Last();
            return extrapolationStepsLastValue;
        }

        protected virtual int[] GetExtrapolationCoefficients(int extrapolationStages)
        {
            return Enumerable.Range(1, extrapolationStages).ToArray();
        }
    }
}