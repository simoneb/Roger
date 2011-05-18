using Common;
using MbUnit.Framework;
using RabbitMQ.Client;

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
        public void Unnamed_queue_should_disappear_when_model_is_closed()
        {
            string queue;

            using (Model)
            {
                queue = Model.QueueDeclare();
            }

            using (Model)
                Assert.AreEqual(0u, Model.QueueDelete(queue));
        }

        [Test]
        public void Queue_should_be_deleted()
        {
            using(Model)
            {
                var queue = Model.QueueDeclare();
                Assert.AreEqual(1u, Model.QueueDelete(queue));
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