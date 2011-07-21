using Common;
using MbUnit.Framework;
using RabbitMQ.Client;
using Rabbus;
using Rabbus.ConsumerToMessageType;
using Rabbus.Reflection;
using Rabbus.RoutingKeys;
using Rabbus.Serialization;
using Rabbus.TypeNames;

namespace Tests.Bus
{
    public abstract class With_default_bus : With_rabbitmq_broker
    {
        protected DefaultBus Bus;
        protected IConnection Connection;
        protected ManualRegistrationConsumerResolver ConsumerResolver;
        private DefaultRoutingKeyGenerator routingKeyGenerator;
        private DefaultTypeNameGenerator typeNameGenerator;
        private ProtoBufNetSerializer serializer;

        [SetUp]
        public void InitializeBus()
        {
            Connection = Helpers.CreateConnection();
            routingKeyGenerator = new DefaultRoutingKeyGenerator();
            typeNameGenerator = new DefaultTypeNameGenerator();
            serializer = new ProtoBufNetSerializer();

            var consumerToMessageTypes = new DefaultConsumerTypeToMessageTypes();
            ConsumerResolver = new ManualRegistrationConsumerResolver(consumerToMessageTypes);
            Bus = new DefaultBus(new IdentityConnectionFactory(Connection),
                                 routingKeyGenerator,
                                 typeNameGenerator,
                                 serializer,
                                 new DefaultReflection(),
                                 ConsumerResolver,
                                 consumerToMessageTypes);

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