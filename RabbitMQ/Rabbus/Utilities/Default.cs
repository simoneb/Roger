using System;
using Rabbus.GuidGeneration;
using Rabbus.Logging;
using Rabbus.Reflection;
using Rabbus.Resolvers;
using Rabbus.Serialization;

namespace Rabbus.Utilities
{
    internal static class Default
    {
        private static readonly Lazy<IConsumerResolver> consumerResolver = new Lazy<IConsumerResolver>(() => new OneWayBusConsumerResolver());
        private static readonly Lazy<ITypeResolver> typeResolver = new Lazy<ITypeResolver>(() => new DefaultTypeResolver());
        private static readonly Lazy<ISupportedMessageTypesResolver> supportedMessageTypesResolver = new Lazy<ISupportedMessageTypesResolver>(() => new DefaultSupportedMessageTypesResolver());
        private static readonly Lazy<IExchangeResolver> exchangeResolver = new Lazy<IExchangeResolver>(() => new DefaultExchangeResolver());
        private static readonly Lazy<IReflection> reflection = new Lazy<IReflection>(() => new DefaultReflection());
        private static readonly Lazy<IRoutingKeyResolver> routingKeyResolver = new Lazy<IRoutingKeyResolver>(() => new DefaultRoutingKeyResolver());
        private static readonly Lazy<IMessageSerializer> serializer = new Lazy<IMessageSerializer>(() => new ProtoBufNetSerializer());
        private static readonly Lazy<IRabbusLog> log = new Lazy<IRabbusLog>(() => new NullLog());
        private static readonly Lazy<IGuidGenerator> guidGenerator = new Lazy<IGuidGenerator>(() => new RandomGuidGenerator());

        public static IConsumerResolver ConsumerResolver { get { return consumerResolver.Value; } }
        public static ITypeResolver TypeResolver { get { return typeResolver.Value; } }
        public static ISupportedMessageTypesResolver SupportedMessageTypesResolver { get { return supportedMessageTypesResolver.Value; } }
        public static IExchangeResolver ExchangeResolver { get { return exchangeResolver.Value; } }
        public static IReflection Reflection { get { return reflection.Value; } }
        public static IRoutingKeyResolver RoutingKeyResolver { get { return routingKeyResolver.Value; } }
        public static IMessageSerializer Serializer { get { return serializer.Value; } }
        public static IRabbusLog Log { get { return log.Value; } }
        public static IGuidGenerator GuidGenerator { get { return guidGenerator.Value; } }
    }
}