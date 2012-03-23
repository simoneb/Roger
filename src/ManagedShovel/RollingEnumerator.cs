using System.Collections;
using System.Collections.Generic;

namespace ManagedShovel
{
    internal class RollingEnumerator<T> : IEnumerator<T>
    {
        private readonly IEnumerable<T> enumerable;
        private IEnumerator<T> enumerator;

        public RollingEnumerator(IEnumerable<T> enumerable)
        {
            this.enumerable = enumerable;
            enumerator = this.enumerable.GetEnumerator();
        }

        public void Dispose()
        {
            enumerator.Dispose();
        }

        public bool MoveNext()
        {
            return enumerator.MoveNext() || (enumerator = enumerable.GetEnumerator()).MoveNext();
        }

        public void Reset()
        {
            enumerator.Reset();
        }

        public T Current
        {
            get { return enumerator.Current; }
        }

        public T Next
        {
            get
            {
                MoveNext();
                return Current;
            }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}