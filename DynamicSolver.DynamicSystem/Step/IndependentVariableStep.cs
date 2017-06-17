using System;

namespace DynamicSolver.DynamicSystem.Step
{
    public struct IndependentVariableStep
    {
        public double FromValue { get; }
        
        public double AbsoluteValue { get; }
        
        public double Delta { get; }
        
        public IndependentVariableStep(double fromValue, double absoluteValue, double delta)
        {
            if (delta < 0) throw new ArgumentOutOfRangeException(nameof(delta));

            FromValue = fromValue;
            AbsoluteValue = absoluteValue;
            Delta = delta;
        }
    }
}