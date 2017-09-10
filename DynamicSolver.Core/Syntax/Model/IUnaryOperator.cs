using JetBrains.Annotations;

namespace DynamicSolver.Core.Syntax.Model
{
    public interface IUnaryOperator : ISyntaxExpression
    {
        [NotNull]
        ISyntaxExpression Operand { get; }
    }
}