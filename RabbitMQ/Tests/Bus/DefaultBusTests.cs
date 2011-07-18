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

            connection.CreateModel().ExchangeDeclare("TestExchange", ExchangeType.Direct, false, true, null);
        }

        [Test]
        public void Test_subscription()
        {
            var consumer = new MyConsumer();

            sut.Subscribe(consumer);

            Thread.Sleep(100);

            sut.Publish(new MyMessage {Value = 1});

            Thread.Sleep(100);

            Assert.AreEqual(1, consumer.Received.Value);
        }

        [Test]
        public void Test_unsubscription()
        {
            var consumer = new MyConsumer();

            var token = sut.Subscribe(consumer);

            Thread.Sleep(100);

            token.Dispose();

            sut.Publish(new MyMessage {Value = 1});

            Thread.Sleep(100);

            Assert.IsNull(consumer.Received);
        }

        [Test]
        public void PublishMandatory_should_invoke_callback()
        {
            var handle = new ManualResetEvent(false);

            sut.PublishMandatory(new MyMessage {Value = 1}, reason => handle.Set());

            if(!handle.WaitOne(100))
                Assert.Fail("Delivery failure callback was not called");
        }

        [Test]
        public void PublishMandatory_should_work_when_message_deliverable()
        {
            var consumer = new MyConsumer();

            sut.Subscribe(consumer);

            Thread.Sleep(100);

            var handle = new ManualResetEvent(false);

            sut.PublishMandatory(new MyMessage {Value = 1}, reason => handle.Set());

            if(handle.WaitOne(100))
                Assert.Fail("Delivery failure callback wasn't expected to be called");

            Assert.IsNotNull(consumer.Received);
        }

        [TearDown]
        public void TearDown()
        {
            connection.Dispose();
        }
    }
}