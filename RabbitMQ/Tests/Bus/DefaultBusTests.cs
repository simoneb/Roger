using System.IO;
using System.Threading;
using Common;
using MbUnit.Framework;
using ProtoBuf;
using RabbitMQ.Client;
using Rabbus;

namespace Tests.Bus
{
    public class DefaultBusTests : With_rabbitmq_broker
    {
        private DefaultBus sut;
        private IConnection connection;
        private DefaultRoutingKeyGenerationStrategy routingKeyGenerationStrategy;
        private TypeNameGenerationStrategy typeNameGenerationStrategy;

        [SetUp]
        public void Setup()
        {
            connection = Helpers.CreateConnection();
            routingKeyGenerationStrategy = new DefaultRoutingKeyGenerationStrategy();
            typeNameGenerationStrategy = new TypeNameGenerationStrategy();
            sut = new DefaultBus(connection, routingKeyGenerationStrategy, typeNameGenerationStrategy);
        }

        [Test]
        public void Test()
        {
            var consumer = new MyConsumer();

            using (var model = connection.CreateModel())
            {
                model.ExchangeDeclare("TestExchange", ExchangeType.Direct, false, true, null);

                sut.Subscribe(consumer);

                Thread.Sleep(100);

                var properties = model.CreateBasicProperties();

                properties.Type = typeNameGenerationStrategy.GetName<MyMessage>();

                using(var s = new MemoryStream())
                {
                    Serializer.Serialize(s, new MyMessage(){Value = 1});
                    model.BasicPublish("TestExchange", routingKeyGenerationStrategy.GetRoutingKey<MyMessage>(), properties, s.ToArray());
                }
            }

            Thread.Sleep(100);

            Assert.AreEqual(1, consumer.Received.Value);
        }

        [TearDown]
        public void TearDown()
        {
            connection.Dispose();
        }
    }
}