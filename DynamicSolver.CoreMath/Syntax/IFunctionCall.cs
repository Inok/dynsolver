using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Syntax
{
    public interface IFunctionCall : IExpression
    {
        [NotNull]
        string FunctionName { get; }
        [NotNull]
        IExpression Argument { get; set; }
    }
}