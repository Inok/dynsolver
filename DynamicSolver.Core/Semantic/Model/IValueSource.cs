using DynamicSolver.Core.Semantic.Model.Type;

namespace DynamicSolver.Core.Semantic.Model
{
    public interface IValueSource : ISemanticElement
    {
        IValueType ValueType { get; }
    }
}