using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbus.Utilities
{
    internal static class EnumerableExtensions
    {
        internal static IEnumerable<T> Return<T>(this T value)
        {
            yield return value;
        }

        internal static IEnumerable<Type> ExceptReplies(this ISet<Type> messageTypes)
        {
            return messageTypes.Where(t => !t.IsDefined(typeof (RabbusReplyAttribute), false));
        }
    }
}