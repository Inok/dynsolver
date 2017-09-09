using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Syntax
{
    public interface IUnaryOperator : IExpression
    {
        [NotNull]
        IExpression Operand { get; }
    }
}