using System;

namespace Roger.Internal
{
    internal static class ErrorMessages
    {
        internal const string ReplyInvokedOutOfRequestContext = "Reply can only be called when handling a request message";

        internal static readonly string ReplyMessageNotAReply = string.Format("Reply message should be decorated with the {0} attribute", typeof (RogerReplyAttribute).Name);

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

        internal static string NormalConsumerOfAbstractClass(Type consumerType, Type messageType)
        {
            return string.Format(@"Consumer {0} cannot consume instances of abstract class {1}.
Use {2} to consume derived classes of base message class", consumerType.Name, messageType.Name, typeof(Consumer<>.SubclassesInSameAssembly));
        }

        internal static string SubclassConsumerOfAbstractClassInHierarchy(Type consumerType, Type messageType)
        {
            return string.Format("Consumer {0} consumes derived classes but there is class {1} in the inheritance chain which is still abstract", 
                                 consumerType.Name, messageType.Name);
        }

        internal static string SubclassConsumerOfNonAbstractClass(Type consumerType, Type messageType)
        {
            return string.Format("Message type {0} should be abstract for consumer {1} to consume its derived classes", messageType.Name, consumerType.Name);
        }
    }
}