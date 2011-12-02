using System;

namespace Rabbus.Errors
{
    internal static class ErrorMessages
    {
        internal const string ReplyInvokedOutOfRequestContext = "Reply can only be called when handling a request message";

        internal static readonly string ReplyMessageNotAReply = string.Format("Reply message should be decorated with the {0} attribute", typeof (RabbusReplyAttribute).Name);

        internal static string MultipleRabbusMessageAttributes(Type messageType)
        {
            return string.Format("Message type {0} has multiple exchanges specified either directly or in his hierarchy", messageType.Name);
        }

        internal static string NoRabbusMessageAttribute(Type messageType)
        {
            return string.Format("Message type {0} should be decorated with {1} attribute", messageType.Name, typeof (RabbusMessageAttribute).Name);
        }

        internal static string InvalidExchangeName(string name)
        {
            return string.Format("Value {0} is not a valid exchange name", name);
        }

        public static string NormalConsumerOfAbstractClass(Type consumerType, Type messageType)
        {
            return string.Format(@"Consumer {0} cannot consume instances of abstract class {1}.
Use {2} to consume derived classes of base message class", consumerType.Name, messageType.Name, typeof(Consumer<>.AndDerivedInSameAssembly));
        }
    }
}