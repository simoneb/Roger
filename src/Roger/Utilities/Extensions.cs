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

        internal static IEnumerable<T> ConcatIf<T>(this IEnumerable<T> input, bool condition, T toConcat)
        {
            if (!condition)
                return input;

            return input.Concat(Return(toConcat));
        }

        internal static IEnumerable<T> Return<T>(this T value)
        {
            yield return value;
        }

        internal static IEnumerable<Type> ConsumerOf(this IEnumerable<Type> messageTypes)
        {
            return messageTypes.Select(m => typeof (IConsumer<>).MakeGenericType(m));
        }
    }
}