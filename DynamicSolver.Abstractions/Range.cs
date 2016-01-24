using System;
using JetBrains.Annotations;

namespace DynamicSolver.Abstractions
{
    public class Range<T> where T : IComparable
    {
        public T From { get; }
        public T To { get; }

        public Range([NotNull] T from, [NotNull] T to)
        {
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (to == null) throw new ArgumentNullException(nameof(to));

            if (from.CompareTo(to) >= 0)
            {
                throw new ArgumentOutOfRangeException($"Provided values is not range because From ({from}) is equal ot greater than To ({to})");
            }

            From = from;
            To = to;
        }
    }
}