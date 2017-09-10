using System;

namespace DynamicSolver.Modelling.Step
{
    public class FixedStepStepper
    {
        private readonly double _stepSize;

        public IndependentVariableStep CurrentStep { get; private set; }

        public FixedStepStepper(double stepSize, double startValue)
        {
            if (stepSize <= 0) throw new ArgumentOutOfRangeException(nameof(stepSize));

            _stepSize = stepSize;
            CurrentStep = new IndependentVariableStep(startValue, startValue, 0);
        }

        public IndependentVariableStep MoveNext()
        {
            var fromValue = CurrentStep.AbsoluteValue;
            var newValue = new IndependentVariableStep(fromValue, fromValue + _stepSize, _stepSize);

            CurrentStep = newValue;

            return newValue;
        }
    }
}