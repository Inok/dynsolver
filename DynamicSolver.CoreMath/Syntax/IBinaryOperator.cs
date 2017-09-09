using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Syntax
{
    public interface IBinaryOperator : IExpression
    {
        [NotNull]
        IExpression LeftOperand { get; }
        [NotNull]
        IExpression RightOperand { get; }        
    }
}