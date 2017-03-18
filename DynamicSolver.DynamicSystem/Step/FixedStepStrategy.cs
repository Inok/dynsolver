using System;

namespace DynamicSolver.DynamicSystem.Step
{
    public class FixedStepStrategy : IIndependentVariableStepStrategy
    {
        private readonly double _stepSize;

        public FixedStepStrategy(double stepSize)
        {
            if (stepSize <= 0) throw new ArgumentOutOfRangeException(nameof(stepSize));
            _stepSize = stepSize;
        }

        public IIndependentVariableStepper Create(double startValue)
        {
            return new FixedStepStepper(startValue, _stepSize);
        }

        private class FixedStepStepper : IIndependentVariableStepper
        {
            private readonly double _stepSize;

            public IndependentVariableStep CurrentStep { get; private set; }

            public FixedStepStepper(double startValue, double stepSize)
            {
                if (stepSize <= 0) throw new ArgumentOutOfRangeException(nameof(stepSize));

                _stepSize = stepSize;
                CurrentStep = new IndependentVariableStep(startValue, 0);
            }

            public IndependentVariableStep MoveNext()
            {
                var currentValue = CurrentStep;

                var newValue = new IndependentVariableStep(currentValue.AbsoluteValue + _stepSize, _stepSize);

                CurrentStep = newValue;

                return newValue;
            }
        }

    }
}