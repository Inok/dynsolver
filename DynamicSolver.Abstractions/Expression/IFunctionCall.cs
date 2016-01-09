namespace DynamicSolver.Abstractions.Expression
{
    public interface IFunctionCall : IExpression
    {
        string FunctionName { get; }
        IExpression Argument { get; set; }
    }
}