using System.Threading;
using Common;
using MbUnit.Framework;
using RabbitMQ.Client;
using Rabbus;

namespace Tests.Bus
{
    public class DefaultBusTests : With_rabbitmq_broker
    {
        private DefaultBus sut;
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

            sut = new DefaultBus(connection, routingKeyGenerator, typeNameGenerator, serializer, new DefaultReflection());
        }

        [Test]
        public void Test_subscription()
        {
            var consumer = new MyConsumer();

            using (var model = connection.CreateModel())
            {
                model.ExchangeDeclare("TestExchange", ExchangeType.Direct, false, true, null);

                sut.Subscribe(consumer);

                Thread.Sleep(100);

                sut.Publish(new MyMessage {Value = 1});

                Thread.Sleep(100);
            }

            Assert.AreEqual(1, consumer.Received.Value);
        }

        [TearDown]
        public void TearDown()
        {
            connection.Dispose();
        }
    }
}