using System;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Model.Type
{
    public class MutualValueTypeResolver
    {
        public static IValueType GetMutualType([NotNull, ItemNotNull] params IValueType[] types)
        {
            if (types == null) throw new ArgumentNullException(nameof(types));
            if (types.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(types));

            var type = types[0];

            for (var i = 1; i < types.Length; i++)
            {
                var checkedType = types[i];
                if (Equals(checkedType, type))
                {
                    continue;
                }

                throw new ArgumentException($"Type mismatch: types '{type}' and '{checkedType}' are incompatible.");
            }
            
            return type;
        }
    }
}