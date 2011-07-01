using System;
using System.Collections.Concurrent;
using System.Threading;
using Common;
using MbUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Linq;

namespace Tests.FunctionalSpecs
{
    public class Shovel_link_goes_down : With_shovel
    {
        private AutoResetEvent publish;
        private int toPublish;
        private ManualResetEvent consumerReady;
        private ConcurrentQueue<int> result;

        [SetUp]
        public void Setup()
        {
            result = new ConcurrentQueue<int>();
            consumerReady = new ManualResetEvent(false);
            publish = new AutoResetEvent(false);
        }

        [Test]
        public void Messages_sent_while_link_is_down_should_be_received_later()
        {
            Start(Consumer);
            consumerReady.WaitOne();

            Start(Publisher);

            for (var i = 0; i < 10; i++)
            {
                Publish(i);

                if (i == 5)
                {
                    ShutdownShovelLink();
                    Thread.Sleep(100); // without this sleep at times the test fails because of message 6 not being received
                }
            }

            Thread.Sleep(2000);

            StartShovelLink();

            Thread.Sleep(5000); // leave some time to shovel to redeliver messages

            SpinWait.SpinUntil(() => result.Count == 10, 2000);

            Publish(-1);

            Assert.AreElementsEqual(Enumerable.Range(0, 10), result);
        }

        private void Publish(int i)
        {
            toPublish = i;
            publish.Set();

            Thread.Sleep(100); // leave some time to consumer to consume
        }

        private void Publisher()
        {
            using(var connection = Helpers.CreateConnection())
            using (var model = connection.CreateModel())
                do
                {
                    publish.WaitOne();

                    model.BasicPublish(Globals.ShovelingExchangeName, "", null, toPublish.Bytes());
                } while (toPublish != -1);
        }

        private void Consumer()
        {
            using(var connection = Helpers.CreateSecondaryConnection())
            using(var model = connection.CreateModel())
            {
                var queue = model.QueueDeclare("", false, true, true, null);
                model.QueueBind(queue, Globals.ShovelingExchangeName, "#");

                var consumer = new QueueingBasicConsumer(model);

                model.BasicConsume(queue, true, consumer);

                consumerReady.Set();

                Func<int> dequeue = () => ((BasicDeliverEventArgs) consumer.Queue.Dequeue()).Body.Integer();
                
                int message;

                while ((message = dequeue()) != -1)
                    result.Enqueue(message);
            }
        }
    }
}