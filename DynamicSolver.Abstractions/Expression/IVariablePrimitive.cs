namespace DynamicSolver.Abstractions.Expression
{
    public interface IVariablePrimitive : IPrimitive
    {
        string Name { get; }        
    }
}