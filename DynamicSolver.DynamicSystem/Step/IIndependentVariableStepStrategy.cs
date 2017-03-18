namespace DynamicSolver.DynamicSystem.Step
{
    public interface IIndependentVariableStepStrategy
    {
        IIndependentVariableStepper Create(double startValue);
    }
}