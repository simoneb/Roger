using System;
using System.Linq;
using Rabbus.Reflection;

namespace Rabbus.Exchanges
{
    public class DefaultExchangeResolver : IExchangeResolver
    {
        private static readonly string[] InvalidChars = new[]{" "};

        public string Resolve(Type messageType)
        {
            EnsureCorrectMessageType(messageType);

            var exchange = messageType.Attribute<RabbusMessageAttribute>().Exchange;

            EnsureCorrectExchangeName(messageType, exchange);

            return exchange;
        }

        private static void EnsureCorrectMessageType(Type messageType)
        {
            if (!messageType.IsDefined(typeof(RabbusMessageAttribute), true))
                throw new InvalidOperationException(string.Format("Message type {0} should be decorated with {1} attribute",
                                                                  messageType.FullName,
                                                                  typeof (RabbusMessageAttribute).FullName));

        }

        private static void EnsureCorrectExchangeName(Type messageType, string exchange)
        {
            if(string.IsNullOrWhiteSpace(exchange) || ContainsInvalidChars(exchange))
                throw new ArgumentException(string.Format(@"Message type {0} does not have a valid exchange name. It can be specified using the {1} attribute",
                                                          messageType.FullName,
                                                          typeof (RabbusMessageAttribute).FullName));
        }

        private static bool ContainsInvalidChars(string exchangeName)
        {
            return InvalidChars.Any(exchangeName.Contains);
        }
    }
}