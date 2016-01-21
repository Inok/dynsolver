namespace DynamicSolver.Abstractions.Expression
{
    public interface IUnaryOperator : IExpression
    {
        IExpression Operand { get; set; }
    }
}