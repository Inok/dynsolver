using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model
{
    public class StructDefinition
    {
        [NotNull, ItemNotNull]
        public IReadOnlyCollection<StructElementDefinition> Elements { get; }

        public StructDefinition([NotNull] params StructElementDefinition[] elements) : this(elements?.AsEnumerable())
        {
        }

        public StructDefinition([NotNull] IEnumerable<StructElementDefinition> elements)
        {
            if (elements == null) throw new ArgumentNullException(nameof(elements));

            var elementList = elements.ToList();
            CheckDuplicates(elementList);

            Elements = new ReadOnlyCollection<StructElementDefinition>(elementList);
        }

        private static void CheckDuplicates(ICollection<StructElementDefinition> elements)
        {
            if (elements.Count == 0)
            {
                throw new ArgumentException("Element list is empty.");
            }
            
            var elementSet = new HashSet<StructElementDefinition>();
            var nameSet = new HashSet<ElementName>();
            foreach (var element in elements)
            {
                if (element == null)
                {
                    throw new ArgumentException("Element list must contain only not-null entries.");
                }

                if (!elementSet.Add(element))
                {
                    throw new ArgumentException("Unable to create a Struct definition with duplicated elements.");
                }

                if (element.ExplicitName != null && !nameSet.Add(element.ExplicitName))
                {
                    throw new ArgumentException($"Unable to create a Struct definition with more than one elements having the same explicit name '{element.ExplicitName}'.");
                }
            }
        }
    }
}