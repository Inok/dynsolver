using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Syntax
{
    public interface IBinaryOperator : ISyntaxExpression
    {
        [NotNull]
        ISyntaxExpression LeftOperand { get; }
        [NotNull]
        ISyntaxExpression RightOperand { get; }        
    }
}