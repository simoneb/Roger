using System;
using System.Linq;

namespace Roger.Internal.Impl
{
    internal class DefaultExchangeResolver : IExchangeResolver
    {
        private static readonly string[] InvalidChars = new[]{" "};

        public string Resolve(Type messageType)
        {
            var exchange = GetExchangeName(messageType);

            EnsureCorrectExchangeName(exchange);

            return exchange;
        }

        public bool IsReply(Type messageType)
        {
            return messageType.IsDefined(typeof (RogerReplyAttribute), false);
        }

        private string GetExchangeName(Type messageType)
        {
            if (IsReply(messageType))
                return GetExchangeName(messageType.GetCustomAttributes(typeof (RogerReplyAttribute), false)
                                                  .Cast<RogerReplyAttribute>()
                                                  .Single().RequestType);

            var attributes = messageType.GetCustomAttributes(typeof (RogerMessageAttribute), true);

            if(attributes.Length == 0)
                throw new InvalidOperationException(ErrorMessages.NoMessageAttribute(messageType));

            if(attributes.Length > 1)
                throw new InvalidOperationException(ErrorMessages.MultipleMessageAttributes(messageType));

            return ((RogerMessageAttribute)attributes.Single()).Exchange;
        }

        private static void EnsureCorrectExchangeName(string exchange)
        {
            if(string.IsNullOrWhiteSpace(exchange) || ContainsInvalidChars(exchange))
                throw new ArgumentException(ErrorMessages.InvalidExchangeName(exchange));
        }

        private static bool ContainsInvalidChars(string exchangeName)
        {
            return InvalidChars.Any(exchangeName.Contains);
        }
    }
}