using MbUnit.Framework;
using RabbitMQ.Client;
using Tests.Integration.Bus.SupportClasses;

namespace Tests.Integration.Bus
{
    public class Send_tests : With_default_bus
    {
        protected override void BeforeBusInitialization()
        {
            TestModel.ExchangeDeclare("SendExchange", ExchangeType.Direct, false, true, null);
        }

        [Test]
        public void Should_send_to_specific_endpoint()
        {
            var consumer = new SendConsumer();
            Bus.AddInstanceSubscription(consumer);
            Bus.Send(Bus.LocalQueue, new SendMessage());

            WaitForDelivery();

            Assert.IsTrue(consumer.Received);
        }
    }
}