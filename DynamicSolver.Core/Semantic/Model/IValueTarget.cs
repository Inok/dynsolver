using DynamicSolver.Core.Semantic.Model.Type;

namespace DynamicSolver.Core.Semantic.Model
{
    public interface IValueTarget : ISemanticElement
    {
        IValueType ValueType { get; }
    }
}