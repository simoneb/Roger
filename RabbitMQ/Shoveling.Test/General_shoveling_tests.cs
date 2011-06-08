using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using MbUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Shoveling.Test
{
    public class General_shoveling_tests : With_shovel
    {
        [Test]
        public void Messages_should_go_from_source_to_destination()
        {
            var handle = new ManualResetEvent(false);

            Task.Factory.StartNew(() => SubscribeAndReceiveOnce(handle, Helpers.CreateSecondaryConnection(), s => Assert.AreEqual("Ciao", s)));

            Task.Factory.StartNew(() => PublishOnce(Helpers.CreateConnection()));

            if (!handle.WaitOne(2000))
                Assert.Fail("Didn't complete in a timely fashion");
        }

        [Test]
        public void Messages_should_not_go_from_destination_to_source()
        {
            var handle = new ManualResetEvent(false);

            Task.Factory.StartNew(() => SubscribeAndReceiveOnce(handle, Helpers.CreateConnection(), s => Assert.Fail("Didn't expect to receive any messages")));

            Task.Factory.StartNew(() => PublishOnce(Helpers.CreateSecondaryConnection()));

            handle.WaitOne(2000);
        }

        private static void SubscribeAndReceiveOnce(EventWaitHandle handle, IConnection connection, Action<string> onMessageReceived)
        {
            using (connection)
            using (var model = connection.CreateModel())
            {
                var queue = model.QueueDeclare("", false, true, true, null);
                model.QueueBind(queue, Globals.ShovelingExchangeName, "#");
                var consumer = new EventingBasicConsumer { Model = model };

                consumer.Received += (sender, args) =>
                {
                    onMessageReceived(args.Body.String());
                    handle.Set();
                };

                model.BasicConsume(queue, true, consumer);
            }
        }

        private static void PublishOnce(IConnection connection)
        {
            using (connection)
            using (var model = connection.CreateModel())
                model.BasicPublish(Globals.ShovelingExchangeName, "", null, "Ciao".Bytes());
        }
    }
}