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

            return Throttle(items, skipItemsCount, 0);
        }

        public static IEnumerable<T> Throttle<T>([NotNull] this IEnumerable<T> items, int skipItemsCount, int firstReturnItemOffset)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (skipItemsCount < 0) throw new ArgumentOutOfRangeException(nameof(skipItemsCount));
            if (firstReturnItemOffset < 0) throw new ArgumentOutOfRangeException(nameof(firstReturnItemOffset));
                        
            return ThrottleIterator(items, skipItemsCount + 1, firstReturnItemOffset);
        }

        private static IEnumerable<T> ThrottleIterator<T>(IEnumerable<T> items, int period, int firstReturnItemOffset)
        {
            int i = -firstReturnItemOffset;
            foreach (var item in items)
            {
                if (i < 0)
                {
                    i++;
                    continue;                    
                }
                if (i == 0)
                {
                    yield return item;
                }
                i = (i + 1) % period;
            }
        }
    }
}