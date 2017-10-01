using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model
{
    public interface IDeclaration
    {
        [CanBeNull]
        ElementName ExplicitName { get; }
    }
}