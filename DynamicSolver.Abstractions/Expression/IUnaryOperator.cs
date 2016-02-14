using JetBrains.Annotations;

namespace DynamicSolver.Abstractions.Expression
{
    public interface IUnaryOperator : IExpression
    {
        [NotNull]
        IExpression Operand { get; set; }
    }
}