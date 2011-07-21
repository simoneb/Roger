using System.Net;
using Common;
using MbUnit.Framework;
using RabbitMQ.Client;
using Tests.Integration.Utils;

namespace Tests.Integration
{
    public class Queues_test : With_rabbitmq_broker
    {
        private IConnection connection;
        private ICustomModel model;

        [SetUp]
        public void Setup()
        {
            connection = Helpers.CreateConnection();
        }

        [Test]
        public void Queue_should_be_deleted()
        {
            using(Model)
            {
                var queue = Model.QueueDeclare();
                Assert.IsNotNull(RestClient.GetQueue(queue));

                Model.QueueDelete(queue);

                Assert.Throws<WebException>(() => RestClient.GetQueue(queue));
            }
        }

        private IModel Model
        {
            get { return model == null || model.Disposed ? (model = new CustomModel(connection)) : model; }
        }

        [TearDown]
        public void Teardown()
        {
            connection.Dispose();
        }
    }
}