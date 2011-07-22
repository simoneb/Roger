using Common;
using MbUnit.Framework;
using RabbitMQ.Client;
using Rabbus;
using Rabbus.Logging;
using Rabbus.Reflection;
using Rabbus.Resolvers;
using Rabbus.Serialization;

namespace Tests.Integration.Bus
{
    public abstract class With_default_bus : With_rabbitmq_broker
    {
        protected DefaultRabbitBus Bus;
        protected IConnection Connection;
        protected ManualRegistrationConsumerResolver ConsumerResolver;
        private DefaultRoutingKeyResolver routingKeyResolver;
        private DefaultTypeResolver typeResolver;
        private ProtoBufNetSerializer serializer;

        [SetUp]
        public void InitializeBus()
        {
            Connection = Helpers.CreateConnection();
            routingKeyResolver = new DefaultRoutingKeyResolver();
            typeResolver = new DefaultTypeResolver();
            serializer = new ProtoBufNetSerializer();

            var consumerToMessageTypes = new DefaultSupportedMessageTypesResolver();
            ConsumerResolver = new ManualRegistrationConsumerResolver(consumerToMessageTypes);
            Bus = new DefaultRabbitBus(new IdentityConnectionFactory(Connection),
                                       ConsumerResolver,
                                       typeResolver, consumerToMessageTypes, new DefaultExchangeResolver(), routingKeyResolver, serializer, new DefaultReflection(), new NullLog());

            BeforeBusInitialization();

            Bus.Initialize();

            Connection.CreateModel().ExchangeDeclare("TestExchange", ExchangeType.Direct, false, true, null);
        }

        protected virtual void BeforeBusInitialization()
        {
        }

        [TearDown]
        public void CloseConnection()
        {
            Connection.Dispose();
        }
    }
}