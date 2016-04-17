using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace DynamicSolver.Abstractions.Tools
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Throttle<T>([NotNull] this IEnumerable<T> items, int skipItemsCount)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (skipItemsCount < 0) throw new ArgumentOutOfRangeException(nameof(skipItemsCount));

            return ThrottleIterator(items, skipItemsCount + 1);
        }

        private static IEnumerable<T> ThrottleIterator<T>(IEnumerable<T> items, int period)
        {
            int i = 0;
            foreach (var item in items)
            {
                if (i == 0)
                {
                    yield return item;
                }
                i = (i + 1) % period;
            }
        }
    }
}