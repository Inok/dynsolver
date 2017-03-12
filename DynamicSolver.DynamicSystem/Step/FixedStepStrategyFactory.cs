using System;

namespace DynamicSolver.DynamicSystem.Step
{
    public class FixedStepStrategyFactory : IIndependentVariableStepStrategyFactory
    {
        private readonly double _stepSize;

        public FixedStepStrategyFactory(double stepSize)
        {
            if (stepSize <= 0) throw new ArgumentOutOfRangeException(nameof(stepSize));
            _stepSize = stepSize;
        }

        public IIndependentVariableStepStrategy Create(double startValue)
        {
            return new FixedStepStrategy(startValue, _stepSize);
        }

        private class FixedStepStrategy : IIndependentVariableStepStrategy
        {
            private readonly double _stepSize;

            public IndependentVariableValue Current { get; private set; }

            public FixedStepStrategy(double startValue, double stepSize)
            {
                if (stepSize <= 0) throw new ArgumentOutOfRangeException(nameof(stepSize));

                _stepSize = stepSize;
                Current = new IndependentVariableValue(startValue, 0);
            }

            public IndependentVariableValue MoveNext()
            {
                var currentValue = Current;

                var newValue = new IndependentVariableValue(currentValue.AbsoluteValue + _stepSize, _stepSize);

                Current = newValue;

                return newValue;
            }
        }

    }
}