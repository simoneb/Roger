using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Common;
using MbUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Shoveling.Test.FunctionalSpecs
{
    public class Subscription_after_beginning_of_session : With_rabbitmq_broker
    {
        private ManualResetEvent exitSemaphore;
        private ManualResetEvent storeReady;
        private ManualResetEvent replayerReady;
        private ManualResetEvent storeHalfWay;

        [SetUp]
        public void Setup()
        {
            exitSemaphore = new ManualResetEvent(false);
            storeReady = new ManualResetEvent(false);
            replayerReady = new ManualResetEvent(false);
            storeHalfWay = new ManualResetEvent(false);
        }

        [TearDown]
        public void TearDown()
        {
            exitSemaphore.Set();            
        }

        [Test]
        public void TEST_NAME()
        {
            Start(MessageStore);
            Start(Producer);

            var messages = Start<IEnumerable<int>>(Consumer);

            Assert.AreEqual(100, messages.Result.Count());
        }

        private IEnumerable<int> Consumer()
        {
            replayerReady.WaitOne();
            storeHalfWay.WaitOne();

            using (var connection = Helpers.CreateConnection())
            using (var model = connection.CreateModel())
            {
                var incomingMessagesQueue = model.QueueDeclare("", false, false, false, null);
                
                var consumer = new QueueingBasicConsumer(model);
                model.BasicConsume(incomingMessagesQueue, true, consumer);

                var basicProperties = model.CreateBasicProperties();
                basicProperties.ReplyTo = incomingMessagesQueue;

                model.BasicPublish("", "MyMessageReplayerQueue", basicProperties, new byte[0]);

                object message;
                int counter = 0;

                while (consumer.Queue.Dequeue(2000, out message))
                {
                    Debug.WriteLine("Consumer received message " + ++counter);

                    var args = message as BasicDeliverEventArgs;

                    yield return args.Body.Integer();
                }
            }
        }

        private void Producer()
        {
            storeReady.WaitOne();

            Debug.WriteLine("Publisher starting");

            using (var connection = Helpers.CreateConnection())
            using (var model = connection.CreateModel())
            {
                model.ExchangeDeclare("MyMessageExchange", ExchangeType.Fanout, false, true, null);

                for (int i = 0; i < 100; i++)
                {
                    model.BasicPublish("MyMessageExchange", "", null, 1.Bytes());
                    Thread.Sleep(200);
                }
            }

            Debug.WriteLine("Publisher completed");
        }

        private void MessageStore()
        {
            using (var connection = Helpers.CreateConnection())
            {
                var messagesInStore = new List<int>();

                Start(() => Store(messagesInStore, connection));

                Start(() => Replayer(messagesInStore, connection));

                exitSemaphore.WaitOne();
            }
        }

        private void Store(List<int> messagesInStore, IConnection connection)
        {
            using (var model = connection.CreateModel())
            {
                var storageQueue = model.QueueDeclare();
                model.ExchangeDeclare("MyMessageExchange", ExchangeType.Fanout, false, true, null);
                model.QueueBind(storageQueue, "MyMessageExchange", "");

                var queueingBasicConsumer = new QueueingBasicConsumer(model);
                model.BasicConsume(storageQueue, true, queueingBasicConsumer);

                storeReady.Set();
                Debug.WriteLine("Store ready");

                while (!exitSemaphore.WaitOne(100))
                {
                    object message;
                    if (queueingBasicConsumer.Queue.Dequeue(100, out message))
                    {
                        Debug.WriteLine("Received new message to store in message store");

                        var args = message as BasicDeliverEventArgs;

                        lock (messagesInStore)
                        {
                            messagesInStore.Add(args.Body.Integer());

                            if (messagesInStore.Count == 50)
                                storeHalfWay.Set();
                        }
                    }
                }
            }
        }

        private void Replayer(List<int> messagesInStore, IConnection connection)
        {
            using (var model = connection.CreateModel())
            {
                var incomingRequests = model.QueueDeclare("MyMessageReplayerQueue", false, true, true, null);

                var consumer = new QueueingBasicConsumer(model);
                model.BasicConsume(incomingRequests, true, consumer);

                replayerReady.Set();
                Debug.WriteLine("Replayer ready");

                while (!exitSemaphore.WaitOne(100))
                {
                    object incomingRequest;

                    if (consumer.Queue.Dequeue(100, out incomingRequest))
                    {
                        Debug.WriteLine("Replayer received new client request");

                        var args = incomingRequest as BasicDeliverEventArgs;

                        lock (messagesInStore)
                            foreach (var message in messagesInStore)
                                model.BasicPublish("", args.BasicProperties.ReplyTo, null, message.Bytes());

                        model.QueueBind(args.BasicProperties.ReplyTo, "MyMessageExchange", "");
                    }
                }
            }
        }
    }
}