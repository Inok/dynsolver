using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace DynamicSolver.App.ViewModel.Common
{
    public static class EnumerableExtensions
    {
        [LinqTunnel]
        public static IEnumerable<T> Skipping<T>([NotNull] this IEnumerable<T> items, int skipCount)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (skipCount < 0) throw new ArgumentOutOfRangeException(nameof(skipCount));

            return Skipping(items, skipCount, 0);
        }

        [LinqTunnel]
        public static IEnumerable<T> Skipping<T>([NotNull] this IEnumerable<T> items, int skipCount, int skipFirst)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (skipCount < 0) throw new ArgumentOutOfRangeException(nameof(skipCount));
            if (skipFirst < 0) throw new ArgumentOutOfRangeException(nameof(skipFirst));

            return SkippingIterator(items, skipCount + 1, skipFirst);
        }

        private static IEnumerable<T> SkippingIterator<T>(IEnumerable<T> items, int period, int skipFirst)
        {
            var i = 0;

            foreach (var item in items.Skip(skipFirst))
            {
                if (i == 0)
                {
                    yield return item;
                }
                i = (i + 1)%period;
            }
        }
    }
}