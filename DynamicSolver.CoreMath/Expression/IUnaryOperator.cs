using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Expression
{
    public interface IUnaryOperator : IExpression
    {
        [NotNull]
        IExpression Operand { get; }
    }
}