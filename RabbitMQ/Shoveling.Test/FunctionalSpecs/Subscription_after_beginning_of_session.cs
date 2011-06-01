using System.Collections;
using System.Collections.Concurrent;
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
        private const int messageNumber = 20;
        private const string Exchange = "MyMessageExchange";
        private const string ReplayerQueue = "MyMessageReplayerQueue";
        private ManualResetEvent storeReady;
        private ManualResetEvent replayerReady;
        private ManualResetEvent storeHalfWay;

        [SetUp]
        public void Setup()
        {
            storeReady = new ManualResetEvent(false);
            replayerReady = new ManualResetEvent(false);
            storeHalfWay = new ManualResetEvent(false);
        }

        [Test]
        public void TEST_NAME()
        {
            var storeTokens = MessageStore().ToArray();
            Start(Producer);

            var consumerResult = Start<IEnumerable<int>>(Consumer);

            Assert.AreEqual(messageNumber, consumerResult.Item1.Result.Count());

            foreach (var token in storeTokens)
                token.Cancel();
        }

        private IEnumerable<int> Consumer()
        {
            replayerReady.WaitOne();
            storeHalfWay.WaitOne();

            Debug.WriteLine("Consumer: starting to listen");

            using (var connection = Helpers.CreateConnection())
            using (var model = connection.CreateModel())
            {
                var incomingMessagesQueue = model.QueueDeclare("", false, false, false, null);
                
                var consumer = new QueueingBasicConsumer(model);
                model.BasicConsume(incomingMessagesQueue, true, consumer);

                var properties = model.CreateBasicProperties();
                properties.ReplyTo = incomingMessagesQueue;

                model.BasicPublish("", ReplayerQueue, properties, new byte[0]);

                object message;
                while (consumer.Queue.Dequeue(5000, out message))
                {
                    var args = (message as BasicDeliverEventArgs);
                    var intMessage = args.Body.Integer();

                    Debug.WriteLine("Consumer: received message " + intMessage + " from " 
                        +  ((byte[])args.BasicProperties.Headers["Sender"]).String());

                    yield return intMessage;
                }

                Debug.WriteLine("Consumer: no message arrived in timely fashion, exiting");
            }
        }

        private void Producer()
        {
            storeReady.WaitOne();

            Debug.WriteLine("Publisher: starting to publish");

            using (var connection = Helpers.CreateConnection())
            using (var model = connection.CreateModel())
            {
                model.ExchangeDeclare(Exchange, ExchangeType.Fanout, false, true, null);

                for (int i = 1; i <= messageNumber; i++)
                {
                    var properties = model.CreateBasicProperties();
                    properties.Headers = new Hashtable {{"Sender", "Producer".Bytes()}};
                    model.BasicPublish(Exchange, "", properties, i.Bytes());
                    Thread.Sleep(150);
                }
            }

            Debug.WriteLine("Publisher: completed publishing");
        }

        private IEnumerable<CancellationTokenSource> MessageStore()
        {
            var messagesInStore = new ConcurrentQueue<BasicDeliverEventArgs>();
            yield return Start(() => Store(messagesInStore));
            yield return Start(() => Replayer(messagesInStore));
        }

        private void Store(ConcurrentQueue<BasicDeliverEventArgs> messagesInStore)
        {
            using (var connection = Helpers.CreateConnection())
            using (var model = connection.CreateModel())
            {
                var storageQueue = model.QueueDeclare();
                model.ExchangeDeclare(Exchange, ExchangeType.Fanout, false, true, null);
                model.QueueBind(storageQueue, Exchange, "");

                var consumer = new QueueingBasicConsumer(model);
                model.BasicConsume(storageQueue, true, consumer);

                storeReady.Set();

                foreach (BasicDeliverEventArgs message in consumer.Queue)
                {
                    Debug.WriteLine("Store: received new message to store");

                    messagesInStore.Enqueue(message);

                    if (messagesInStore.Count == messageNumber/2)
                        storeHalfWay.Set();

                    //Thread.Sleep(100);
                }
            }
        }

        private void Replayer(IEnumerable<BasicDeliverEventArgs> messagesInStore)
        {
            using (var connection = Helpers.CreateConnection())
            using (var model = connection.CreateModel())
            {
                var incomingRequestsQueue = model.QueueDeclare(ReplayerQueue, false, true, true, null);

                var consumer = new QueueingBasicConsumer(model);
                model.BasicConsume(incomingRequestsQueue, true, consumer);

                replayerReady.Set();

                foreach (BasicDeliverEventArgs incomingRequest in consumer.Queue)
                {
                    model.QueueBind(incomingRequest.BasicProperties.ReplyTo, Exchange, "");

                    Debug.WriteLine("Replayer: received new client request");

                    foreach (var message in messagesInStore)
                    {
                        var properties = message.BasicProperties;
                        properties.Headers["Sender"] = "Store".Bytes();

                        model.BasicPublish("", incomingRequest.BasicProperties.ReplyTo, properties, message.Body);
                    }
                }
            }
        }
    }
}