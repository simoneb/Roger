using System.Threading;
using Common;
using MbUnit.Framework;
using RabbitMQ.Client.MessagePatterns;

namespace Tests.Integration.Federation
{
    public class Subscriber_on_secondary_node : With_federation
    {
        private ManualResetEvent consumerReady;

        [SetUp]
        public void Setup()
        {
            consumerReady = new ManualResetEvent(false);
        }

        [Test]
        public void Should_receive_messages_published_on_main_node_after_consumer_subscription()
        {
            var consumer = Start<int>(OneConsumer);

            Start(OneProducer);

            Assert.AreEqual(1, consumer.Item1.Result);
        }

        private void OneProducer()
        {
            using (var connection = Helpers.CreateConnection())
            using (var model = connection.CreateModel())
            {
                consumerReady.WaitOne();

                int counter = 0;
                model.BasicPublish(Globals.FederationExchangeName, "", null, (++counter).Bytes());
            }
        }

        private int OneConsumer()
        {
            using (var connection = Helpers.CreateSecondaryConnectionToSecondaryVirtualHost())
            using (var model = connection.CreateModel())
            {
                var queue = model.QueueDeclare();
                model.QueueBind(queue, Globals.FederationExchangeName, "#");
                var subscription = new Subscription(model, queue, true);

                consumerReady.Set();

                return subscription.Next().Body.Integer();
            }
        }
    }
}