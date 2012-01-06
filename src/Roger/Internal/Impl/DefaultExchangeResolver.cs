using System;
using System.Linq;
using Roger.Utilities;

namespace Roger.Internal.Impl
{
    /// <summary>
    /// Default implementation of the <see cref="IExchangeResolver"/> interface.
    /// </summary>
    /// <remarks>
    /// Supports exchanges defined using the <see cref="RogerMessageAttribute"/> and inherited classes,
    /// and propagates them along the inheritance chain. 
    /// Multiple attributes along the inheritance chain are not supported, even if they specify the same exchange.
    /// </remarks>
    internal class DefaultExchangeResolver : IExchangeResolver
    {
        private static readonly string[] InvalidChars = new[]{" "};

        public string Resolve(Type messageType)
        {
            var exchange = GetExchangeName(messageType);

            EnsureCorrectExchangeName(exchange);

            return exchange;
        }

        private static string GetExchangeName(Type messageType)
        {
            if (messageType.IsReply())
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