using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using MbUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Shoveling.Test
{
    public class With_shovel : With_rabbitmq_broker
    {
        [FixtureSetUp]
        public void Setup()
        {
            Broker.AddVHost("secondary");
            Broker.AddPermissions("secondary", "guest");
            Broker.StopApp();
            Thread.Sleep(1000);
            Broker.StartAppAndWait();
        }

        [FixtureTearDown]
        public void Teardown()
        {
            Broker.DeleteVHost("secondary");
        }

        [Test]
        public void Messages_should_go_from_source_to_destination()
        {
            var handle = new ManualResetEvent(false);

            Task.Factory.StartNew(() => Subscribe(handle, Helpers.CreateSecondaryConnection(), s => Assert.AreEqual("Ciao", s)));

            Task.Factory.StartNew(() => Publish(Helpers.CreateConnection()));

            if(!handle.WaitOne(2000))
                Assert.Fail("Didn't complete in a timely fashion");
        }

        [Test]
        public void Messages_should_not_go_from_destination_to_source()
        {
            var handle = new ManualResetEvent(false);

            Task.Factory.StartNew(() => Subscribe(handle, Helpers.CreateConnection(), s => Assert.Fail("Didn't expect to receive any messages")));

            Task.Factory.StartNew(() => Publish(Helpers.CreateSecondaryConnection()));

            handle.WaitOne(2000);
        }

        private static void Subscribe(EventWaitHandle handle, IConnection connection, Action<string> onMessageReceived)
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

        private static void Publish(IConnection connection)
        {
            using (connection)
            using (var model = connection.CreateModel())
                model.BasicPublish(Globals.ShovelingExchangeName, "", null, "Ciao".Bytes());
        }
    }
}