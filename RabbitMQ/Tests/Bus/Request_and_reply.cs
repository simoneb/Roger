using System.Threading;
using MbUnit.Framework;
using ProtoBuf;
using RabbitMQ.Client;
using Rabbus;

namespace Tests.Bus
{
    public class Request_and_reply : With_default_bus
    {
        [SetUp]
        public void Setup()
        {
            connection.CreateModel().ExchangeDeclare("RequestExchange", ExchangeType.Direct, false, true, null);
        }

        [Test]
        public void Should_send_request_to_request_consumer()
        {
            var requestConsumer = new MyRequestConsumer(Bus);

            Bus.AddInstanceSubscription(requestConsumer);

            Bus.Request(new MyRequest());

            Thread.Sleep(1000);

            Assert.IsNotNull(requestConsumer.Received);
        }

        [Test]
        public void Should_reply_to_response_consumer()
        {
            var requestConsumer = new MyRequestConsumer(Bus);
            var responseConsumer = new MyResponseConsumer();

            Bus.AddInstanceSubscription(requestConsumer);
            Bus.AddInstanceSubscription(responseConsumer);

            Bus.Request(new MyRequest());

            Thread.Sleep(1000);

            Assert.IsNotNull(responseConsumer.Received);
        }
    }

    public class MyResponseConsumer : IConsumer<MyResponse>
    {
        public MyResponse Received;

        public void Consume(MyResponse message)
        {
            Received = message;
        }
    }

    public class MyRequestConsumer : IConsumer<MyRequest>
    {
        private readonly IRabbitBus m_bus;
        public MyRequest Received;

        public MyRequestConsumer(IRabbitBus bus)
        {
            m_bus = bus;
        }

        public void Consume(MyRequest message)
        {
            Received = message;
            m_bus.Reply(new MyResponse());
        }
    }

    [RabbusMessage("RequestExchange")]
    [ProtoContract]
    public class MyResponse
    {
    }

    [RabbusMessage("RequestExchange")]
    [ProtoContract]
    public class MyRequest
    {
    }
}