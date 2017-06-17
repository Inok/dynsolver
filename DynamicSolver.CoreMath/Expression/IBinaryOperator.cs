using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Expression
{
    public interface IBinaryOperator : IExpression
    {
        [NotNull]
        IExpression LeftOperand { get; }
        [NotNull]
        IExpression RightOperand { get; }        
    }
}