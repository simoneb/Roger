using System;
using Common;
using MbUnit.Framework;
using RabbitMQ.Client;
using Rabbus;

namespace Tests.Bus
{
    public abstract class With_default_bus : With_rabbitmq_broker
    {
        protected DefaultBus Bus;
        protected IConnection connection;
        private DefaultRoutingKeyGenerator routingKeyGenerator;
        private TypeNameGenerator typeNameGenerator;
        private ProtoBufNetSerializer serializer;
        protected FakeConsumerResolver ConsumerResolver;

        [SetUp]
        public void InitializeBus()
        {
            connection = Helpers.CreateConnection();
            routingKeyGenerator = new DefaultRoutingKeyGenerator();
            typeNameGenerator = new TypeNameGenerator();
            serializer = new ProtoBufNetSerializer();

            var consumerToMessageTypes = new DefaultConsumerTypeToMessageTypes();
            ConsumerResolver = new FakeConsumerResolver(consumerToMessageTypes);
            Bus = new DefaultBus(connection, routingKeyGenerator, typeNameGenerator, serializer, new DefaultReflection(),
                                 ConsumerResolver, consumerToMessageTypes);

            BeforeBusInitialization();

            Bus.Initialize();

            connection.CreateModel().ExchangeDeclare("TestExchange", ExchangeType.Direct, false, true, null);
        }

        protected virtual void BeforeBusInitialization()
        {
        }

        [TearDown]
        public void CloseConnection()
        {
            connection.Dispose();
        }
    }

}