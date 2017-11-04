namespace DynamicSolver.Core.Semantic.Model
{
    public interface ISemanticElement
    {
        T Accept<T>(ISemanticVisitor<T> visitor);
    }
}