using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using MbUnit.Framework;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;

namespace Shoveling.Test.FunctionalSpecs
{
    [TestFixture]
    public class As_a_client : With_shovel
    {
        
    }

    public class On_secondary_node : As_a_client
    {
        private ManualResetEvent m_consumerReadyHandle;

        [SetUp]
        public void Setup()
        {
            m_consumerReadyHandle = new ManualResetEvent(false);
        }

        [Test]
        public void Should_receive_messages_published_on_main_node_after_subscription()
        {
            var consumer = Start<BasicDeliverEventArgs>(Consumer);
            Start(Producer);

            Assert.AreEqual(1, consumer.Result.Body.Integer());
        }

        [Test]
        public void Should_receive_messages_sent_while_not_subscribed()
        {
            
        }

        private void Producer()
        {
            using (var connection = Helpers.CreateConnection())
            using (var model = connection.CreateModel())
            {
                m_consumerReadyHandle.WaitOne();

                int counter = 0;
                model.BasicPublish(Globals.ShovelingExchangeName, "", null, (++counter).Bytes());
            }
        }

        private BasicDeliverEventArgs Consumer()
        {
            using (var connection = Helpers.CreateSecondaryConnection())
            using (var model = connection.CreateModel())
            {
                var queue = model.QueueDeclare();
                model.QueueBind(queue, Globals.ShovelingExchangeName, "#");
                var subscription = new Subscription(model, queue, true);

                m_consumerReadyHandle.Set();

                return subscription.Next();
            }
        }
    }
}