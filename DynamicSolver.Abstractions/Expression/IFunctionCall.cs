using JetBrains.Annotations;

namespace DynamicSolver.Abstractions.Expression
{
    public interface IFunctionCall : IExpression
    {
        [NotNull]
        string FunctionName { get; }
        [NotNull]
        IExpression Argument { get; set; }
    }
}