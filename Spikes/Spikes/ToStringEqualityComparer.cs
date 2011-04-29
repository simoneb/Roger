using System.Collections.Generic;

namespace Spikes
{
    internal class ToStringEqualityComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            return x.ToString().Equals(y.ToString());
        }

        public int GetHashCode(T obj)
        {
            return obj.ToString().GetHashCode();
        }
    }
}