using System.Threading;
using System.Threading.Tasks;
using Common;
using MbUnit.Framework;
using RabbitMQ.Client.Events;

namespace Shoveling.Test
{
    public class With_shovel
    {
        [FixtureSetUp]
        public void Setup()
        {
            //Broker.Instance.AddVHost("secondary");
            //Broker.Instance.AddPermissions("secondary", "guest");
        }

        [FixtureTearDown]
        public void Teardown()
        {
            //Broker.Instance.DeleteVHost("secondary");
        }

        [Test]
        public void Messages_should_go_from_source_to_destination()
        {
            var handle = new ManualResetEvent(false);

            Task.Factory.StartNew(() => Subscribe(handle));

            Task.Factory.StartNew(Publish);

            handle.WaitOne();
        }

        private static void Subscribe(EventWaitHandle handle)
        {
            using (var connection = Helpers.CreateSecondaryConnection())
            using (var model = connection.CreateModel())
            {
                var queue = model.QueueDeclare("", false, true, true, null);
                model.QueueBind(queue, Globals.ShovelingExchangeName, "#");
                var consumer = new EventingBasicConsumer { Model = model };

                consumer.Received += (sender, args) =>
                {
                    Assert.AreEqual("Ciao", args.Body.String());
                    handle.Set();
                };

                model.BasicConsume(queue, true, consumer);
            }
        }

        private static void Publish()
        {
            using (var connection = Helpers.CreateConnection())
            using (var model = connection.CreateModel())
                model.BasicPublish(Globals.ShovelingExchangeName, "", null, "Ciao".Bytes());
        }
    }
}