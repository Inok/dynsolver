using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Syntax
{
    public interface IUnaryOperator : ISyntaxExpression
    {
        [NotNull]
        ISyntaxExpression Operand { get; }
    }
}