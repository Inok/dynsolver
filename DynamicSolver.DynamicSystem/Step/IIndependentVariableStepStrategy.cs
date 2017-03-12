namespace DynamicSolver.DynamicSystem.Step
{
    public interface IIndependentVariableStepStrategy
    {
        IndependentVariableValue Current { get; }
        IndependentVariableValue MoveNext();
    }
}