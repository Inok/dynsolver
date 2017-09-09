using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Syntax.Model
{
    public interface IFunctionCall : ISyntaxExpression
    {
        [NotNull]
        string FunctionName { get; }
        [NotNull]
        ISyntaxExpression Argument { get; set; }
    }
}