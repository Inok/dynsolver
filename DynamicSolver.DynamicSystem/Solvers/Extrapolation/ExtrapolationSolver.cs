using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.DynamicSystem.Step;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem.Solvers.Extrapolation
{
    public class ExtrapolationSolver : IDynamicSystemSolver
    {
        private readonly bool _parallelize;

        public DynamicSystemSolverDescription Description { get; }

        [NotNull]
        protected IDynamicSystemSolver BaseSolver { get; }

        protected int ExtrapolationStages { get; }

        public ExtrapolationSolver([NotNull] IDynamicSystemSolver baseSolver, int extrapolationStages, bool parallelize = false)
        {
            _parallelize = parallelize;
            if (baseSolver == null) throw new ArgumentNullException(nameof(baseSolver));
            if (extrapolationStages <= 0) throw new ArgumentOutOfRangeException(nameof(extrapolationStages));

            BaseSolver = baseSolver;
            ExtrapolationStages = extrapolationStages;

            var order = baseSolver.Description.Order + (extrapolationStages - 1) * (baseSolver.Description.IsSymmetric ? 2 : 1);
            var name = $"{order}-order extrapolation method ({extrapolationStages}-staged, based on {baseSolver.Description.Name})";
            Description = new DynamicSystemSolverDescription(name, order, false);
        }

        public IEnumerable<DynamicSystemState> Solve(IExplicitOrdinaryDifferentialEquationSystem equationSystem, 
            DynamicSystemState initialState,
            ModellingTaskParameters parameters)
        {
            if (equationSystem == null) throw new ArgumentNullException(nameof(equationSystem));
            if (initialState == null) throw new ArgumentNullException(nameof(initialState));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            var stepper = new FixedStepStepper(parameters.Step, initialState.IndependentVariable);

            var extrapolationCoefficients = GetExtrapolationCoefficients(ExtrapolationStages);
            var buffer = new double[ExtrapolationStages, ExtrapolationStages];
            var solvesBuffer = new IReadOnlyDictionary<string, double>[ExtrapolationStages];

            var lastState = initialState;

            while (true)
            {
                var step = stepper.MoveNext();

                if (_parallelize)
                {
                    var state = lastState;
                    foreach (var vars in extrapolationCoefficients
                        .Select((c, i) => new KeyValuePair<int, int>(i, c))
                        .AsParallel()
                        .Select(pair => new KeyValuePair<int, DynamicSystemState>(pair.Key, MakeExtrapolationSteps(equationSystem, state, step, pair.Value)))
                        .AsSequential())
                    {
                        solvesBuffer[vars.Key] = vars.Value.DependentVariables;
                    }
                }
                else
                {
                    for (var i = 0; i < solvesBuffer.Length; i++)
                    {
                        solvesBuffer[i] = MakeExtrapolationSteps(equationSystem, lastState, step, extrapolationCoefficients[i]).DependentVariables;
                    }
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

        protected virtual DynamicSystemState MakeExtrapolationSteps(IExplicitOrdinaryDifferentialEquationSystem equationSystem, DynamicSystemState state, IndependentVariableStep step, int extrapolationCoefficient)
        {
            var stepSize = step.Delta / extrapolationCoefficient;
            var extrapolationStepsLastValue = BaseSolver.Solve(equationSystem, state, new ModellingTaskParameters(stepSize)).Take(extrapolationCoefficient).Last();
            return extrapolationStepsLastValue;
        }

        protected virtual int[] GetExtrapolationCoefficients(int extrapolationStages)
        {
            var coefficients = new int[extrapolationStages];
            if (BaseSolver.Description.UseEvenExtrapolationCoefficients)
            {
                for (var i = 0; i < coefficients.Length; i++)
                {
                    coefficients[i] = (i + 1)*2;
                }
            }
            else
            {
                for (var i = 0; i < coefficients.Length; i++)
                {
                    coefficients[i] = i + 1;
                }
                
            }
            return coefficients;
        }
    }
}