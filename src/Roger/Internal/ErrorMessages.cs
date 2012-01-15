using System;

namespace Roger.Internal
{
    internal static class ErrorMessages
    {
        internal const string ReplyInvokedOutOfRequestContext = "Reply can only be called when handling a request message";

        internal static string MultipleMessageAttributes(Type messageType)
        {
            return string.Format("Message type {0} has multiple exchanges specified either directly or in his hierarchy", messageType.Name);
        }

        internal static string NoMessageAttribute(Type messageType)
        {
            return string.Format("Message type {0} should be decorated with {1} attribute", messageType.Name, typeof (RogerMessageAttribute).Name);
        }

        internal static string InvalidExchangeName(string name)
        {
            return string.Format("Value {0} is not a valid exchange name", name);
        }
    }
}