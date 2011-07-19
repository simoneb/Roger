using System.Threading;
using MbUnit.Framework;
using RabbitMQ.Client;

namespace Tests.Bus
{
    public class Consumer_resolution : With_default_bus
    {
        private MyConsumer consumer;

        protected override void BeforeBusInitialization()
        {
            consumer = new MyConsumer();
            ConsumerResolver.Register(consumer);
            connection.CreateModel().ExchangeDeclare("TestExchange", ExchangeType.Direct, false, true, null);
        }

        [Test]
        public void Publish()
        {
            Bus.Publish(new MyMessage());

            Thread.Sleep(1000);

            Assert.IsNotNull(consumer.Received);
        }
    }
}