using System;
using MbUnit.Framework;
using RabbitMQ.Client;
using Rabbus.Errors;
using Tests.Integration.Bus.SupportClasses;

namespace Tests.Integration.Bus
{
    public class Request_and_reply : With_default_bus
    {
        protected override void BeforeBusInitialization()
        {
            TestModel.ExchangeDeclare("RequestExchange", ExchangeType.Direct, false, true, null);
        }

        [Test]
        public void Should_send_request_to_request_consumer()
        {
            var responder = new MyRequestResponder(Bus);

            Bus.AddInstanceSubscription(responder);

            Bus.Request(new MyRequest());

            WaitForRoundtrip();

            Assert.IsNotNull(responder.Received);
        }

        [Test]
        public void Should_reply_to_response_consumer()
        {
            var responder = new MyRequestResponder(Bus);
            var responseConsumer = new GenericConsumer<MyReply>();

            Bus.AddInstanceSubscription(responder);
            Bus.AddInstanceSubscription(responseConsumer);

            Bus.Request(new MyRequest());

            responseConsumer.WaitForDelivery();

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

            WaitForRoundtrip();

            Assert.AreEqual("RequestExchange", responseConsumer.CurrentMessage.Exchange);
        }

        [Test]
        public void Should_not_require_exchange_to_be_defined_on_reply_message_type()
        {
            var responseConsumer = new GenericConsumer<MyReply>();

            Bus.AddInstanceSubscription(responseConsumer);
        }

        [Test]
        [Ignore("This is no longer the expected behavior, who knows whether it will make sense once again one day")]
        public void Should_throw_if_more_than_one_consumer_can_receive_the_reply()
        {
            var responder = new MyRequestResponder(Bus);
            var responseConsumer1 = new GenericConsumer<MyReply>();
            var responseConsumer2 = new GenericConsumer<MyReply>();

            Bus.AddInstanceSubscription(responder);
            Bus.AddInstanceSubscription(responseConsumer1);
            Bus.AddInstanceSubscription(responseConsumer2);

            AggregateException error = null;
            Bus.Request(new MyRequest(), _ => {});

            WaitForRoundtrip();

            Assert.IsNull(responseConsumer1.LastReceived);
            Assert.IsNull(responseConsumer2.LastReceived);
            Assert.IsNotNull(error);
        }

        [Test]
        public void Reply_should_throw_if_invoked_out_of_the_context_of_handling_a_message()
        {
            Assert.Throws<InvalidOperationException>(() => Bus.Reply(new MyReply()));
        }

        [Test]
        public void Reply_should_throw_if_invoked_out_of_the_context_of_handling_a_request()
        {
            var responder = new CatchingResponder<MyRequest, MyReply>(Bus);
            Bus.AddInstanceSubscription(responder);

            Bus.Publish(new MyRequest());

            WaitForRoundtrip();

            Assert.IsInstanceOfType<InvalidOperationException>(responder.Exception);
            Assert.AreEqual(ErrorMessages.ReplyInvokedOutOfRequestContext, responder.Exception.Message);
        }

        [Test]
        public void Reply_should_throw_if_message_is_not_decorated_with_the_correct_attribute()
        {
            var responder = new CatchingResponder<MyRequest, MyWrongReply>(Bus);
            Bus.AddInstanceSubscription(responder);

            Bus.Request(new MyRequest());

            WaitForRoundtrip();

            Assert.IsInstanceOfType<InvalidOperationException>(responder.Exception);
            Assert.AreEqual(ErrorMessages.ReplyMessageNotAReply, responder.Exception.Message);
        }
    }
}