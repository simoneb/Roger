using System.Threading;
using System.Threading.Tasks;
using Common;
using MbUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Tests.Integration.Federation
{
    public class General_federation_tests : With_federation
    {
        private static ManualResetEvent _consumerReady;

        [SetUp]
        public void Setup()
        {
            _consumerReady = new ManualResetEvent(false);
        }

        [Test]
        public void Messages_should_go_from_source_to_destination()
        {
            var consumer = Task.Factory.StartNew(() => SubscribeAndReceiveOnce(Helpers.CreateSecondaryConnectionToSecondaryVirtualHost()));

            Task.Factory.StartNew(() => PublishOnce(Helpers.CreateConnection()));

            if(!consumer.Wait(2000))
                Assert.Fail("Did not complete in timely fashion");

            Assert.AreEqual("Ciao", consumer.Result);
        }

        [Test]
        public void Messages_should_not_go_from_destination_to_source()
        {
            var handle = new ManualResetEvent(false);

            Task.Factory.StartNew(() => SubscribeAndReceiveOnce(Helpers.CreateConnection()));

            Task.Factory.StartNew(() => PublishOnce(Helpers.CreateSecondaryConnectionToSecondaryVirtualHost()));

            handle.WaitOne(2000);
        }

        private static string SubscribeAndReceiveOnce(IConnection connection)
        {
            using (connection)
            using (var model = connection.CreateModel())
            {
                var queue = model.QueueDeclare("", false, false, true, null);
                model.QueueBind(queue, Globals.FederationExchangeName, "#");
                var consumer = new QueueingBasicConsumer(model);

                model.BasicConsume(queue, true, consumer);

                _consumerReady.Set();

                return ((BasicDeliverEventArgs)consumer.Queue.Dequeue()).Body.String();
            }
        }

        private static void PublishOnce(IConnection connection)
        {
            using (connection)
            using (var model = connection.CreateModel())
            {
                _consumerReady.WaitOne();

                model.BasicPublish(Globals.FederationExchangeName, "ciao", null, "Ciao".Bytes());
            }
        }
    }
}