using System;
using System.Collections.Generic;
using Roger.Internal;
using Roger.Internal.Impl;

namespace Roger
{
    internal static class Default
    {
        private static readonly Lazy<IConsumerContainer> DefaultConsumerResolver = new Lazy<IConsumerContainer>(() => new EmptyConsumerContainer());
        private static readonly Lazy<ITypeResolver> DefaultTypeResolver = new Lazy<ITypeResolver>(() => new DefaultTypeResolver());
        private static readonly Lazy<ISupportedMessageTypesResolver> DefaultSupportedMessageTypesResolver = new Lazy<ISupportedMessageTypesResolver>(() => new DefaultSupportedMessageTypesResolver());
        private static readonly Lazy<IExchangeResolver> DefaultExchangeResolver = new Lazy<IExchangeResolver>(() => new DefaultExchangeResolver());
        private static readonly Lazy<IReflection> DefaultReflection = new Lazy<IReflection>(() => new DefaultReflection());
        private static readonly Lazy<IRoutingKeyResolver> DefaultRoutingKeyResolver = new Lazy<IRoutingKeyResolver>(() => new DefaultRoutingKeyResolver());
        private static readonly Lazy<IBindingKeyResolver> DefaultBindingKeyResolver = new Lazy<IBindingKeyResolver>(() => new DefaultBindingKeyResolver(DefaultRoutingKeyResolver.Value));
        private static readonly Lazy<IMessageSerializer> DefaultSerializer = new Lazy<IMessageSerializer>(() => new ProtoBufNetSerializer());
        private static readonly Lazy<IRogerLog> DefaultLog = new Lazy<IRogerLog>(() => new NullLog());
        private static readonly Lazy<IIdGenerator> DefaultGuidGenerator = new Lazy<IIdGenerator>(() => new RandomIdGenerator());
        private static readonly Lazy<ISequenceGenerator> DefaultSequenceGenerator = new Lazy<ISequenceGenerator>(() => new ThreadSafeIncrementalSequenceGenerator());
        private static readonly Lazy<IConsumerInvoker> DefaultConsumerInvoker = new Lazy<IConsumerInvoker>(() => new AlwaysSuccessConsumerInvoker(DefaultReflection.Value));

        private static readonly Lazy<IEnumerable<IMessageFilter>> DefaultFilters =
            new Lazy<IEnumerable<IMessageFilter>>(() => new IMessageFilter[]
            {
                new DeduplicationFilter(TimeSpan.FromMinutes(2)),
                new ResequencingFilter()
            });

        public static IConsumerContainer ConsumerContainer { get { return DefaultConsumerResolver.Value; } }
        public static ITypeResolver TypeResolver { get { return DefaultTypeResolver.Value; } }
        public static ISupportedMessageTypesResolver SupportedMessageTypesResolver { get { return DefaultSupportedMessageTypesResolver.Value; } }
        public static IExchangeResolver ExchangeResolver { get { return DefaultExchangeResolver.Value; } }
        public static IReflection Reflection { get { return DefaultReflection.Value; } }
        public static IRoutingKeyResolver RoutingKeyResolver { get { return DefaultRoutingKeyResolver.Value; } }
        public static IBindingKeyResolver BindingKeyResolver { get { return DefaultBindingKeyResolver.Value; } }
        public static IMessageSerializer Serializer { get { return DefaultSerializer.Value; } }
        public static IRogerLog Log { get { return DefaultLog.Value; } }
        public static IIdGenerator IdGenerator { get { return DefaultGuidGenerator.Value; } }
        public static IEnumerable<IMessageFilter> Filters { get { return DefaultFilters.Value; } }
        public static ISequenceGenerator SequenceGenerator { get { return DefaultSequenceGenerator.Value; } }
        public static IConsumerInvoker ConsumerInvoker { get { return DefaultConsumerInvoker.Value; } }
    }
}