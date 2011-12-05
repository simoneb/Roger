using System;

namespace Rabbus.Utilities
{
    internal static class EnumerableExtensions
    {
        internal static bool IsReply(this Type messageType)
        {
            return messageType.IsDefined(typeof (RabbusReplyAttribute), false);
        }
    }
}