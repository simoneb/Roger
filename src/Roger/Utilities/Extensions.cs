using System;
using System.Collections.Generic;
using System.Linq;

namespace Roger.Utilities
{
    internal static class Extensions
    {
        internal static bool IsReply(this Type messageType)
        {
            return messageType.IsDefined(typeof (RogerReplyAttribute), false);
        }

        internal static IEnumerable<Type> ConsumersOf(this IEnumerable<Type> messageTypes)
        {
            return messageTypes.Select(m => typeof (IConsumer<>).MakeGenericType(m));
        }

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