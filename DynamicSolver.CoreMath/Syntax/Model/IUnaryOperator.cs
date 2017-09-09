using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Syntax.Model
{
    public interface IUnaryOperator : ISyntaxExpression
    {
        [NotNull]
        ISyntaxExpression Operand { get; }
    }
}