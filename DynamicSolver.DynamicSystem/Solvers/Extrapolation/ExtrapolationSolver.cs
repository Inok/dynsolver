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

        [NotNull] private readonly IDynamicSystemSolver _baseSolver;
        private readonly int _extrapolationStages;

        public ExtrapolationSolver([NotNull] IDynamicSystemSolver baseSolver, int extrapolationStages)
        {
            if (baseSolver == null) throw new ArgumentNullException(nameof(baseSolver));
            if (extrapolationStages <= 0) throw new ArgumentOutOfRangeException(nameof(extrapolationStages));

            _baseSolver = baseSolver;
            _extrapolationStages = extrapolationStages;
            
            var name = $"Extrapolation method ({extrapolationStages}-staged, based on {baseSolver.Description.Name})";
            var order = baseSolver.Description.Order + (extrapolationStages - 1) * (baseSolver.Description.IsSymmetric ? 2 : 1);
            Description = new DynamicSystemSolverDescription(name, order, false);
        }

        public IEnumerable<DynamicSystemState> Solve(IExplicitOrdinaryDifferentialEquationSystem equationSystem, IIndependentVariableStepStrategyFactory stepStrategyFactory)
        {
            if (equationSystem == null) throw new ArgumentNullException(nameof(equationSystem));
            if (stepStrategyFactory == null) throw new ArgumentNullException(nameof(stepStrategyFactory));

            var stepStrategy = stepStrategyFactory.Create(equationSystem.InitialState.IndependentVariable);

            var extrapolationCoefficients = Enumerable.Range(1, _extrapolationStages).ToArray();
            var buffer = new double[_extrapolationStages, _extrapolationStages];
            var solvesBuffer = new IReadOnlyDictionary<string, double>[_extrapolationStages];

            var lastState = equationSystem.InitialState;
            while (true)
            {
                var previousIndependentVariableValue = stepStrategy.Current.AbsoluteValue;

                var step = stepStrategy.MoveNext();

                for (var i = 0; i < solvesBuffer.Length; i++)
                {
                    var state = equationSystem.WithInitialState(new DynamicSystemState(previousIndependentVariableValue, lastState.DependentVariables));

                    var extrapolationCoefficient = extrapolationCoefficients[i];
                    var stepSize = step.Delta / extrapolationCoefficient;
                    solvesBuffer[i] = _baseSolver.Solve(state, new FixedStepStrategyFactory(stepSize)).Take(extrapolationCoefficient).Last().DependentVariables;
                }

                var newValues = new Dictionary<string, double>();

                foreach (var variable in lastState.DependentVariables.Keys)
                {
                    for (var j = 0; j < _extrapolationStages; j++)
                    {
                        buffer[j, 0] = solvesBuffer[j][variable];

                        for (var k = 0; k < j; k++)
                        {
                            buffer[j, k + 1] = buffer[j, k] + (buffer[j, k] - buffer[j - 1, k]) / (Math.Pow((double)extrapolationCoefficients[j] / extrapolationCoefficients[j - k - 1], _baseSolver.Description.Order) - 1);
                        }
                    }

                    newValues[variable] = buffer[_extrapolationStages - 1, _extrapolationStages - 1];
                }

                yield return lastState = new DynamicSystemState(step.AbsoluteValue, newValues);
            }
        }
    }
}