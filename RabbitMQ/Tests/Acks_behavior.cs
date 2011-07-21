using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Common;
using MbUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Tests.Integration
{
    public class Acks_behavior : With_rabbitmq_broker
    {
        private int NumberOfMessagesToProduce = 10;
        private ConcurrentQueue<int> m_received;
        private IEnumerable<int> ExpectedMessages = Enumerable.Range(0, 10);
        private string m_queueName;
        private Action<IModel, ulong> OnReceived;
        private int NumberOfExpectedMessages;

        [SetUp]
        public void Setup()
        {
            NumberOfExpectedMessages = NumberOfMessagesToProduce;
            OnReceived = delegate { };
            m_received = new ConcurrentQueue<int>();
        }

        [Test]
        public void Auto_ack()
        {
            Start(() => Consumer(true));

            Thread.Sleep(100);

            Start(Producer);

            SpinWait.SpinUntil(ReceivedAllMessages, 100);

            Assert.AreElementsEqual(ExpectedMessages, m_received);
        }

        [Test]
        public void Explicit_ack()
        {
            OnReceived = (model, deliveryTag) =>
            {
                if (m_received.Count == 1)
                    model.BasicAck(deliveryTag, false);
            };

            Start(() => Consumer(false));

            Thread.Sleep(100);

            Start(Producer);

            SpinWait.SpinUntil(ReceivedAllMessages, 2000);

            Assert.AreElementsEqual(ExpectedMessages, m_received);
        }

        [Test]
        public void Explicit_ack_and_recover()
        {
            NumberOfExpectedMessages = 20;

            OnReceived = (model, deliveryTag) =>
            {
                if (m_received.Count == 10)
                    // apparently passing false is not supported
                    model.BasicRecover(true);
            };

            Start(() => Consumer(false));

            Thread.Sleep(100);

            Start(Producer);

            SpinWait.SpinUntil(ReceivedAllMessages, 2000);

            Assert.AreElementsEqual(2.Times(ExpectedMessages), m_received);
        }

        [Test]
        public void Nack_with_requeue()
        {
            NumberOfExpectedMessages = 11;

            ulong toRedeliverSince = 0;

            OnReceived = (model, deliveryTag) =>
            {
                if (m_received.Count == 1)
                    toRedeliverSince = deliveryTag;
                    
                if(m_received.Count == 10)
                    model.BasicNack(toRedeliverSince, false, true);
            };

            Start(() => Consumer(false));

            Thread.Sleep(100);

            Start(Producer);

            SpinWait.SpinUntil(ReceivedAllMessages, 2000);

            Assert.AreElementsEqual(ExpectedMessages.Concat(new[]{0}), m_received);
        }

        [Test]
        public void Reject_with_requeue()
        {
            NumberOfExpectedMessages = 11;

            ulong toRedeliverSince = 0;

            OnReceived = (model, deliveryTag) =>
            {
                if (m_received.Count == 1)
                    toRedeliverSince = deliveryTag;
                    
                if(m_received.Count == 10)
                    model.BasicReject(toRedeliverSince, true);
            };

            Start(() => Consumer(false));

            Thread.Sleep(100);

            Start(Producer);

            SpinWait.SpinUntil(ReceivedAllMessages, 2000);

            Assert.AreElementsEqual(ExpectedMessages.Concat(new[] { 0 }), m_received);
        }

        [Test]
        public void Nack_with_multiple_and_requeue()
        {
            NumberOfExpectedMessages = 15;

            ulong toRedeliverSince = 0;

            OnReceived = (model, deliveryTag) =>
            {
                if (m_received.Count == 5)
                    toRedeliverSince = deliveryTag;

                if(m_received.Count == 10)
                    model.BasicNack(toRedeliverSince, true, true);
            };

            Start(() => Consumer(false));

            Thread.Sleep(100);

            Start(Producer);

            SpinWait.SpinUntil(ReceivedAllMessages, 2000);

            Assert.AreElementsEqual(ExpectedMessages.Concat(new[] { 4, 3, 2, 1, 0 }), m_received);
        }

        private bool ReceivedAllMessages()
        {
            return m_received.Count == NumberOfExpectedMessages;
        }

        private void Consumer(bool noAck)
        {
            using (var connection = Helpers.CreateConnection())
            using (var model = connection.CreateModel())
            {
                m_queueName = model.QueueDeclare("", false, false, true, null);

                var consumer = new QueueingBasicConsumer(model);

                model.BasicConsume(m_queueName, noAck, consumer);

                foreach (BasicDeliverEventArgs _ in consumer.Queue)
                {
                    m_received.Enqueue(_.Body.Integer());

                    OnReceived(model, _.DeliveryTag);
                }
            }
        }
        
        private void Producer()
        {
            using (var connection = Helpers.CreateConnection())
            using (var model = connection.CreateModel())
            {
                for (int i = 0; i < NumberOfMessagesToProduce; i++)
                    model.BasicPublish("", m_queueName, null, i.Bytes());
            }
        }
    }

    public static class Extension
    {
        public static IEnumerable<T> Times<T>(this int times, IEnumerable<T> enumerable)
        {
            IEnumerable<T> result = enumerable;

            for (int i = 1; i < times;i++)
                result = result.Concat(enumerable);

            return result;
        }
    }
}