using System;
using System.Collections.Generic;

namespace Roger.Utilities
{
    internal static class Extensions
    {
       
        internal static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var entry in enumerable)
                action(entry);
        }

        internal static IEnumerable<T> Where<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> key, Func<TKey, TKey, bool> filter, TKey firstPrevious = default(TKey))
        {
            var previousKey = firstPrevious;

            foreach (var current in enumerable)
            {
                if(filter(previousKey, key(current)))
                {
                    yield return current;
                    previousKey = key(current);
                }
                else
                    yield break;
            }
        }
    }
}