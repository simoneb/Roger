using System;
using MbUnit.Framework;
using RabbitMQ.Client;
using Tests.Integration.Bus.SupportClasses;

namespace Tests.Integration.Bus
{
    public class Request_and_reply : With_default_bus
    {
        protected override void BeforeBusInitialization()
        {
            TestModel.ExchangeDeclare("RequestExchange", ExchangeType.Topic, false, true, null);
        }

        [Test]
        public void Should_send_request_to_request_consumer()
        {
            var responder = new MyRequestResponder(Bus);

            Bus.AddInstanceSubscription(responder);

            Bus.Request(new MyRequest());

            Assert.IsTrue(responder.WaitForDelivery());
            Assert.IsNotNull(responder.LastReceived);
        }

        [Test]
        public void Should_reply_to_response_consumer()
        {
            var responder = new MyRequestResponder(Bus);
            var responseConsumer = new GenericConsumer<MyReply>();

            Bus.AddInstanceSubscription(responder);
            Bus.AddInstanceSubscription(responseConsumer);

            Bus.Request(new MyRequest());

            Assert.IsTrue(responseConsumer.WaitForDelivery(1500 /*roudtrip here*/));
            Assert.IsNotNull(responseConsumer.LastReceived);
        }

        [Test]
        public void Reply_should_come_through_exchange_defined_by_response_message()
        {
            var responder = new MyRequestResponder(Bus);
            var responseConsumer = new MyResponseCurrentMessageConsumer(Bus);

            Bus.AddInstanceSubscription(responder);
            Bus.AddInstanceSubscription(responseConsumer);

            Bus.Request(new MyRequest());

            Assert.IsTrue(responseConsumer.WaitForDelivery());

            Assert.AreEqual("RequestExchange", responseConsumer.CurrentMessage.Exchange);
        }

        [Test]
        public void Should_not_require_exchange_to_be_defined_on_reply_message_type()
        {
            var responseConsumer = new GenericConsumer<MyReply>();

            Bus.AddInstanceSubscription(responseConsumer);
        }

        [Test]
        public void Reply_should_throw_if_invoked_out_of_the_context_of_handling_a_message()
        {
            Assert.Throws<InvalidOperationException>(() => Bus.Reply(new MyReply()));
        }
    }
}