using System;
using System.Threading;
using MbUnit.Framework;
using RabbitMQ.Client;
using Rabbus.Errors;

namespace Tests.Integration.Bus.RequestReply
{
    public class Request_and_reply : With_default_bus
    {
        protected override void BeforeBusInitialization()
        {
            Connection.CreateModel().ExchangeDeclare("RequestExchange", ExchangeType.Direct, false, true, null);
        }

        [Test]
        public void Should_send_request_to_request_consumer()
        {
            var responder = new MyRequestResponder(Bus);

            Bus.AddInstanceSubscription(responder);

            Bus.Request(new MyRequest());

            WaitForDelivery();

            Assert.IsNotNull(responder.Received);
        }

        [Test]
        public void Should_reply_to_response_consumer()
        {
            var responder = new MyRequestResponder(Bus);
            var responseConsumer = new MyResponseConsumer();

            Bus.AddInstanceSubscription(responder);
            Bus.AddInstanceSubscription(responseConsumer);

            Bus.Request(new MyRequest());

            WaitForDelivery();

            Assert.IsNotNull(responseConsumer.Received);
        }

        [Test]
        public void Reply_should_come_through_exchange_defined_by_response_message()
        {
            var responder = new MyRequestResponder(Bus);
            var responseConsumer = new MyResponseCurrentMessageConsumer(Bus);

            Bus.AddInstanceSubscription(responder);
            Bus.AddInstanceSubscription(responseConsumer);

            Bus.Request(new MyRequest());

            WaitForDelivery();

            Assert.AreEqual("RequestExchange", responseConsumer.CurrentMessage.Exchange);
        }

        [Test]
        public void Should_not_require_exchange_to_be_defined_on_reply_message_type()
        {
            var responseConsumer = new MyResponseConsumer();

            Bus.AddInstanceSubscription(responseConsumer);
        }

        [Test]
        public void Should_throw_if_more_than_one_consumer_can_receive_the_reply()
        {
            var responder = new MyRequestResponder(Bus);
            var responseConsumer1 = new MyResponseConsumer();
            var responseConsumer2 = new MyResponseConsumer();

            Bus.AddInstanceSubscription(responder);
            Bus.AddInstanceSubscription(responseConsumer1);
            Bus.AddInstanceSubscription(responseConsumer2);

            AggregateException error = null;
            Bus.Request(new MyRequest(), _ => {}, reason => error = reason.Exception);

            WaitForDelivery();

            Assert.IsNull(responseConsumer1.Received);
            Assert.IsNull(responseConsumer2.Received);
            Assert.IsNotNull(error);
        }

        [Test]
        public void Reply_should_throw_if_invoked_out_of_the_context_of_handling_a_message()
        {
            Assert.Throws<InvalidOperationException>(() => Bus.Reply(new MyResponse()));
        }

        [Test]
        public void Reply_should_throw_if_invoked_out_of_the_context_of_handling_a_request()
        {
            var responder = new CatchingMyRequestResponder(Bus);
            Bus.AddInstanceSubscription(responder);

            Bus.Publish(new MyRequest());

            WaitForDelivery();

            Assert.IsInstanceOfType<InvalidOperationException>(responder.Exception);
            Assert.AreEqual(ErrorMessages.ReplyInvokedOutOfRequestContext, responder.Exception.Message);
        }

        private static void WaitForDelivery()
        {
            Thread.Sleep(400);
        }
    }
}