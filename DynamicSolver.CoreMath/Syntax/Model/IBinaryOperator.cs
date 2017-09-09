using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Syntax.Model
{
    public interface IBinaryOperator : ISyntaxExpression
    {
        [NotNull]
        ISyntaxExpression LeftOperand { get; }
        [NotNull]
        ISyntaxExpression RightOperand { get; }        
    }
}