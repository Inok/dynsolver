using JetBrains.Annotations;

namespace DynamicSolver.Core.Syntax.Model
{
    public interface IBinaryOperator : ISyntaxExpression
    {
        [NotNull]
        ISyntaxExpression LeftOperand { get; }
        [NotNull]
        ISyntaxExpression RightOperand { get; }        
    }
}