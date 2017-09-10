namespace DynamicSolver.Core.Semantic
{
    public interface ISemanticElement
    {
        T Accept<T>(ISemanticVisitor<T> visitor);
    }
}