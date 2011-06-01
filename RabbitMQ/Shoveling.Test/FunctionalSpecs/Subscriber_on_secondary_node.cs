using System.Threading;
using Common;
using MbUnit.Framework;
using RabbitMQ.Client.MessagePatterns;

namespace Shoveling.Test.FunctionalSpecs
{
    public class Subscriber_on_secondary_node : With_shovel
    {
        private ManualResetEvent m_consumerReady;

        [SetUp]
        public void Setup()
        {
            m_consumerReady = new ManualResetEvent(false);
        }

        [Test]
        public void Should_receive_messages_published_on_main_node_after_subscription()
        {
            var consumer = Start<int>(OneConsumer);
            Start(OneProducer);

            Assert.AreEqual(1, consumer.Result);
        }

        private void OneProducer()
        {
            using (var connection = Helpers.CreateConnection())
            using (var model = connection.CreateModel())
            {
                m_consumerReady.WaitOne();

                int counter = 0;
                model.BasicPublish(Globals.ShovelingExchangeName, "", null, (++counter).Bytes());
            }
        }

        private int OneConsumer()
        {
            using (var connection = Helpers.CreateSecondaryConnection())
            using (var model = connection.CreateModel())
            {
                var queue = model.QueueDeclare();
                model.QueueBind(queue, Globals.ShovelingExchangeName, "#");
                var subscription = new Subscription(model, queue, true);

                m_consumerReady.Set();

                return subscription.Next().Body.Integer();
            }
        }
    }
}