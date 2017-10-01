using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model
{
    public class StructElementDefinition
    {
        [CanBeNull]
        public ElementName ExplicitName { get; }

        public StructElementDefinition()
        {
        }

        public StructElementDefinition([NotNull] string explicitName)
        {
            ExplicitName = new ElementName(explicitName);
        }
    }
}