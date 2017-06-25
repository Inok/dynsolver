namespace DynamicSolver.CoreMath.Semantic
{
    public interface ISemanticElement
    {
        T Accept<T>(ISemanticVisitor<T> visitor);
    }
}