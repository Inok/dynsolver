using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace DynamicSolver.LinearAlgebra
{
    public class GenericEnumerator<T> : IEnumerator<T>
    {
        private readonly IEnumerator _enumerator;

        public GenericEnumerator([NotNull] IEnumerator enumerator)
        {
            if (enumerator == null) throw new ArgumentNullException(nameof(enumerator));
            _enumerator = enumerator;
        }

        public T Current => (T) _enumerator.Current;

        object IEnumerator.Current => _enumerator.Current;

        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        public void Reset()
        {
            _enumerator.Reset();
        }

        public void Dispose()
        {

        }
    }
}