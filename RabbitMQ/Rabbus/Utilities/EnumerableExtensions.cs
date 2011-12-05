using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbus.Utilities
{
    internal static class EnumerableExtensions
    {
        internal static IEnumerable<Type> ExceptReplies(this IEnumerable<Type> messageTypes)
        {
            return messageTypes.Where(t => !t.IsDefined(typeof (RabbusReplyAttribute), false));
        }
    }
}