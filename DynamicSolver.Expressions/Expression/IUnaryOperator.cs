using JetBrains.Annotations;

namespace DynamicSolver.Expressions.Expression
{
    public interface IUnaryOperator : IExpression
    {
        [NotNull]
        IExpression Operand { get; }
    }
}