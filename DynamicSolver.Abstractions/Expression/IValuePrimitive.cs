namespace DynamicSolver.Abstractions.Expression
{
    public interface IValuePrimitive : IPrimitive
    {
        double Value { get; }
    }
}