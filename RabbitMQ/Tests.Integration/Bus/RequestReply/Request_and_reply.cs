using System;
using System.Threading;
using MbUnit.Framework;
using RabbitMQ.Client;

namespace Tests.Integration.Bus.RequestReply
{
    public class Request_and_reply : With_default_bus
    {
        [SetUp]
        public void Setup()
        {
            Connection.CreateModel().ExchangeDeclare("RequestExchange", ExchangeType.Direct, false, true, null);
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

        [Test]
        public void Should_throw_if_more_than_one_consumer_can_receive_the_reply()
        {
            var requestConsumer = new MyRequestConsumer(Bus);
            var responseConsumer1 = new MyResponseConsumer();
            var responseConsumer2 = new MyResponseConsumer();

            Bus.AddInstanceSubscription(requestConsumer);
            Bus.AddInstanceSubscription(responseConsumer1);
            Bus.AddInstanceSubscription(responseConsumer2);

            AggregateException error = null;
            Bus.Request(new MyRequest(), reason => error = reason.Exception);

            Thread.Sleep(1000);

            Assert.IsNull(responseConsumer1.Received);
            Assert.IsNull(responseConsumer2.Received);
            Assert.IsNotNull(error);
        }
    }
}