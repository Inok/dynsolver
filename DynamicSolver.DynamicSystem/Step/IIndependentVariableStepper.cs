namespace DynamicSolver.DynamicSystem.Step
{
    public interface IIndependentVariableStepper
    {
        IndependentVariableStep CurrentStep { get; }
        IndependentVariableStep MoveNext();
    }
}