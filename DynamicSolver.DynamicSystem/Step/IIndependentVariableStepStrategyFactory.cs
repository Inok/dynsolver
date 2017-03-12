namespace DynamicSolver.DynamicSystem.Step
{
    public interface IIndependentVariableStepStrategyFactory
    {
        IIndependentVariableStepStrategy Create(double startValue);
    }
}