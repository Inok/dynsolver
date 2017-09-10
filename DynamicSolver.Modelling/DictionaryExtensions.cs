using System.Collections.Generic;

namespace DynamicSolver.Modelling
{
    public static class DictionaryExtensions
    {
        public static void CopyTo<TKey, TValue>(this IDictionary<TKey,TValue> from, IDictionary<TKey, TValue> target)
        {
            foreach (var pair in from)
            {
                target[pair.Key] = pair.Value;
            }
        }

        public static void CopyTo<TKey, TValue>(this IReadOnlyDictionary<TKey,TValue> from, IDictionary<TKey, TValue> target)
        {
            foreach (var pair in from)
            {
                target[pair.Key] = pair.Value;
            }
        }

        public static Dictionary<TKey, TValue> Clone<TKey, TValue>(this IReadOnlyDictionary<TKey,TValue> from)
        {
            var target = new Dictionary<TKey, TValue>();
            from.CopyTo(target);
            return target;
        }

        public static Dictionary<TKey, TValue> Clone<TKey, TValue>(this IDictionary<TKey,TValue> from)
        {
            var target = new Dictionary<TKey, TValue>();
            from.CopyTo(target);
            return target;
        }
    }
}