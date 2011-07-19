using Common;
using MbUnit.Framework;
using RabbitMQ.Client;
using Rabbus;

namespace Tests.Bus
{
    public abstract class With_default_bus : With_rabbitmq_broker
    {
        protected IRabbitBus Bus;
        protected IConnection connection;
        private DefaultRoutingKeyGenerator routingKeyGenerator;
        private TypeNameGenerator typeNameGenerator;
        private ProtoBufNetSerializer serializer;

        [SetUp]
        public void InitializeBus()
        {
            connection = Helpers.CreateConnection();
            routingKeyGenerator = new DefaultRoutingKeyGenerator();
            typeNameGenerator = new TypeNameGenerator();
            serializer = new ProtoBufNetSerializer();

            var consumerToMessageTypes = new DefaultConsumerToMessageTypes();
            Bus = new DefaultBus(connection, routingKeyGenerator, typeNameGenerator, serializer, new DefaultReflection(), new FakeConsumerResolver(consumerToMessageTypes), consumerToMessageTypes);

            connection.CreateModel().ExchangeDeclare("TestExchange", ExchangeType.Direct, false, true, null);
        }

        [TearDown]
        public void CloseConnection()
        {
            connection.Dispose();
        }
    }

}