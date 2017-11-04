using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model
{
    public interface ISemanticElement
    {
        T Accept<T>([NotNull] ISemanticVisitor<T> visitor);
    }
}