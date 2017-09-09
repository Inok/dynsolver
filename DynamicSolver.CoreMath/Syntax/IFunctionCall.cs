using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Syntax
{
    public interface IFunctionCall : ISyntaxExpression
    {
        [NotNull]
        string FunctionName { get; }
        [NotNull]
        ISyntaxExpression Argument { get; set; }
    }
}