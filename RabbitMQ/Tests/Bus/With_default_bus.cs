using Common;
using MbUnit.Framework;
using RabbitMQ.Client;
using Rabbus;

namespace Tests.Bus
{
    public abstract class With_default_bus : With_rabbitmq_broker
    {
        protected DefaultBus Bus;
        private IConnection connection;
        private DefaultRoutingKeyGenerator routingKeyGenerator;
        private TypeNameGenerator typeNameGenerator;
        private ProtoBufNetSerializer serializer;

        [SetUp]
        public void Setup()
        {
            connection = Helpers.CreateConnection();
            routingKeyGenerator = new DefaultRoutingKeyGenerator();
            typeNameGenerator = new TypeNameGenerator();
            serializer = new ProtoBufNetSerializer();

            Bus = new DefaultBus(connection, routingKeyGenerator, typeNameGenerator, serializer, new DefaultReflection());

            connection.CreateModel().ExchangeDeclare("TestExchange", ExchangeType.Direct, false, true, null);
        }

        [TearDown]
        public void TearDown()
        {
            connection.Dispose();
        }
    }
}