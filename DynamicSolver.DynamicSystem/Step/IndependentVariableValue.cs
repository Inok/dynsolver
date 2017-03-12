using System;

namespace DynamicSolver.DynamicSystem.Step
{
    public struct IndependentVariableValue
    {
        public double AbsoluteValue { get; }
        public double Delta { get; }

        public IndependentVariableValue(double absoluteValue, double delta)
        {
            if (delta < 0) throw new ArgumentOutOfRangeException(nameof(delta));

            AbsoluteValue = absoluteValue;
            Delta = delta;
        }
    }
}