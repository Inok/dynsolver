using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Collections
{
    internal class UniqueKeyValueSet<TItem1, TItem2>
    {
        private readonly Dictionary<TItem1, TItem2> _item1ToItem2;
        private readonly Dictionary<TItem2, TItem1> _item2ToItem1;

        public TItem2 this[TItem1 key] => _item1ToItem2[key];
        public TItem1 this[TItem2 key] => _item2ToItem1[key];

        public UniqueKeyValueSet() : this(null, null)
        {
        }

        public UniqueKeyValueSet(IEqualityComparer<TItem1> item1Comparer, IEqualityComparer<TItem2> item2Comparer)
        {
            _item1ToItem2 = new Dictionary<TItem1, TItem2>(item1Comparer);
            _item2ToItem1 = new Dictionary<TItem2, TItem1>(item2Comparer);
        }

        public void Add([NotNull] TItem1 item1, [NotNull] TItem2 item2)
        {
            if (item1 == null) throw new ArgumentNullException(nameof(item1));
            if (item2 == null) throw new ArgumentNullException(nameof(item2));

            if (_item1ToItem2.ContainsKey(item1) || _item2ToItem1.ContainsKey(item2))
            {
                throw new InvalidOperationException("Cannot add new entry to set: it is already added.");
            }

            _item1ToItem2.Add(item1, item2);
            _item2ToItem1.Add(item2, item1);
        }

        public bool ContainsItem1(TItem1 item)
        {
            return _item1ToItem2.ContainsKey(item);
        }

        public bool ContainsItem2(TItem2 item)
        {
            return _item2ToItem1.ContainsKey(item);
        }

        public bool TryGetValueByItem1(TItem1 key, out TItem2 value)
        {
            return _item1ToItem2.TryGetValue(key, out value);
        }

        public bool TryGetValueByItem2(TItem2 key, out TItem1 value)
        {
            return _item2ToItem1.TryGetValue(key, out value);
        }
    }
}