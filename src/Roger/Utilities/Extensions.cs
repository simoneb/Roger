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
    }
}