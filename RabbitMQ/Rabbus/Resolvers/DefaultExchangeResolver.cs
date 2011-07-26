using System;
using System.Linq;

namespace Rabbus.Resolvers
{
    public class DefaultExchangeResolver : IExchangeResolver
    {
        private static readonly string[] InvalidChars = new[]{" "};

        public string Resolve(Type messageType)
        {
            EnsureCorrectMessageType(messageType);

            var exchange = GetExchangeName(messageType);

            EnsureCorrectExchangeName(exchange);

            return exchange;
        }

        private static void EnsureCorrectMessageType(Type messageType)
        {
            if (!messageType.IsDefined(typeof(RabbusMessageAttribute), true))
                throw new InvalidOperationException(string.Format("Message type {0} should be decorated with {1} attribute",
                                                                  messageType.FullName,
                                                                  typeof (RabbusMessageAttribute).FullName));

        }

        private static string GetExchangeName(Type messageType)
        {
            return ((RabbusMessageAttribute) messageType.GetCustomAttributes(typeof (RabbusMessageAttribute), false).Single()).Exchange;
        }

        private static void EnsureCorrectExchangeName(string exchange)
        {
            if(string.IsNullOrWhiteSpace(exchange) || ContainsInvalidChars(exchange))
                throw new ArgumentException(string.Format(@"Value {0} is not a valid exchange name",
                                                          exchange));
        }

        private static bool ContainsInvalidChars(string exchangeName)
        {
            return InvalidChars.Any(exchangeName.Contains);
        }
    }
}