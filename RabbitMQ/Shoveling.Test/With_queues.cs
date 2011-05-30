using System.Net;
using Common;
using MbUnit.Framework;
using RabbitMQ.Client;
using Shoveling.Test.Utils;

namespace Shoveling.Test
{
    public class With_queues : With_rabbitmq_broker
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
                Assert.IsNotNull(Client.GetQueue(queue));

                Model.QueueDelete(queue);

                Assert.Throws<WebException>(() => Client.GetQueue(queue));
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