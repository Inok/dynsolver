using System;

namespace DynamicSolver.DynamicSystem.Step
{
    public struct IndependentVariableStep
    {
        public double AbsoluteValue { get; }
        public double Delta { get; }

        public IndependentVariableStep(double absoluteValue, double delta)
        {
            if (delta < 0) throw new ArgumentOutOfRangeException(nameof(delta));

            AbsoluteValue = absoluteValue;
            Delta = delta;
        }
    }
}