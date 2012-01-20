using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Common;
using MbUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Tests.Integration.Exploratory
{
    public class Acks_behavior : With_rabbitmq_broker
    {
        private const int NumberOfMessagesToProduce = 10;
        private ConcurrentQueue<int> received;
        private readonly IEnumerable<int> expectedMessages = Enumerable.Range(0, 10);
        private string queueName;
        private Action<IModel, ulong> onReceived;
        private int numberOfExpectedMessages;

        [SetUp]
        public void Setup()
        {
            numberOfExpectedMessages = NumberOfMessagesToProduce;
            onReceived = delegate { };
            received = new ConcurrentQueue<int>();
        }

        [Test]
        public void Auto_ack()
        {
            Start(() => Consumer(true));

            Thread.Sleep(100);

            Start(Producer);

            SpinWait.SpinUntil(ReceivedAllMessages, 500);

            Assert.AreElementsEqual(expectedMessages, received);
        }

        [Test]
        public void Explicit_ack()
        {
            onReceived = (model, deliveryTag) =>
            {
                if (received.Count == 1)
                    model.BasicAck(deliveryTag, false);
            };

            Start(() => Consumer(false));

            Thread.Sleep(100);

            Start(Producer);

            SpinWait.SpinUntil(ReceivedAllMessages, 2000);

            Assert.AreElementsEqual(expectedMessages, received);
        }

        [Test]
        public void Explicit_ack_and_recover()
        {
            numberOfExpectedMessages = 20;

            onReceived = (model, deliveryTag) =>
            {
                if (received.Count == 10)
                    // apparently passing false is not supported
                    model.BasicRecover(true);
            };

            Start(() => Consumer(false));

            Thread.Sleep(100);

            Start(Producer);

            SpinWait.SpinUntil(ReceivedAllMessages, 2000);

            Assert.AreElementsEqual(2.Times(expectedMessages), received);
        }

        [Test]
        public void Nack_with_requeue()
        {
            numberOfExpectedMessages = 11;

            ulong toRedeliverSince = 0;

            onReceived = (model, deliveryTag) =>
            {
                if (received.Count == 1)
                    toRedeliverSince = deliveryTag;
                    
                if(received.Count == 10)
                    model.BasicNack(toRedeliverSince, false, true);
            };

            Start(() => Consumer(false));

            Thread.Sleep(100);

            Start(Producer);

            SpinWait.SpinUntil(ReceivedAllMessages, 2000);

            Assert.AreElementsEqual(expectedMessages.Concat(new[]{0}), received);
        }

        [Test]
        public void Reject_with_requeue()
        {
            numberOfExpectedMessages = 11;

            ulong toRedeliverSince = 0;

            onReceived = (model, deliveryTag) =>
            {
                if (received.Count == 1)
                    toRedeliverSince = deliveryTag;
                    
                if(received.Count == 10)
                    model.BasicReject(toRedeliverSince, true);
            };

            Start(() => Consumer(false));

            Thread.Sleep(100);

            Start(Producer);

            SpinWait.SpinUntil(ReceivedAllMessages, 2000);

            Assert.AreElementsEqual(expectedMessages.Concat(new[] { 0 }), received);
        }

        [Test]
        public void Nack_with_multiple_and_requeue_requeues_nacked_messages_in_the_order_they_were_published()
        {
            numberOfExpectedMessages = 15;

            ulong toRedeliverSince = 0;

            onReceived = (model, deliveryTag) =>
            {
                if (received.Count == 5)
                    toRedeliverSince = deliveryTag;

                if(received.Count == 10)
                    model.BasicNack(toRedeliverSince, true, true);
            };

            Start(() => Consumer(false));

            Thread.Sleep(100);

            Start(Producer);

            SpinWait.SpinUntil(ReceivedAllMessages, 2000);

            Assert.AreElementsEqual(expectedMessages.Concat(new[] { 0, 1, 2, 3, 4 }), received);
        }

        private bool ReceivedAllMessages()
        {
            return received.Count == numberOfExpectedMessages;
        }

        private void Consumer(bool noAck)
        {
            using (var connection = Helpers.CreateSafeShutdownConnection())
            using (var model = connection.CreateModel())
            {
                queueName = model.QueueDeclare("", false, false, true, null);

                var consumer = new QueueingBasicConsumer(model);

                model.BasicConsume(queueName, noAck, consumer);

                foreach (BasicDeliverEventArgs _ in consumer.Queue)
                {
                    received.Enqueue(_.Body.Integer());

                    onReceived(model, _.DeliveryTag);
                }
            }
        }
        
        private void Producer()
        {
            using (var connection = Helpers.CreateSafeShutdownConnection())
            using (var model = connection.CreateModel())
            {
                for (int i = 0; i < NumberOfMessagesToProduce; i++)
                    model.BasicPublish("", queueName, null, i.Bytes());
            }
        }
    }
}