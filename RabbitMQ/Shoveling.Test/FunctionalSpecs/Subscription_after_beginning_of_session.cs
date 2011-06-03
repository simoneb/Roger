using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Common;
using MbUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Util;
using Shoveling.Test.Utils;

namespace Shoveling.Test.FunctionalSpecs
{
    public class Subscription_after_beginning_of_session : With_rabbitmq_broker
    {
        private const int messageNumber = 20;
        private const string Exchange = "MyMessageExchange";
        private const string ReplayerQueue = "MyMessageReplayerQueue";
        private const string SourceHeader = "X-Source";
        private const string LiveQueueHeader = "X-LiveQueue";
        private ManualResetEvent storeReady;
        private ManualResetEvent replayerReady;
        private ManualResetEvent storeHalfWay;
        private IMessageStore storage;

        [SetUp]
        public void Setup()
        {
            storeReady = new ManualResetEvent(false);
            replayerReady = new ManualResetEvent(false);
            storeHalfWay = new ManualResetEvent(false);
        }

        [TearDown]
        public void Teardown()
        {
            using (var connection = Helpers.CreateConnection())
            using (var model = connection.CreateModel())
            {
                model.ExchangeDelete(Exchange);
                model.QueueDelete(ReplayerQueue);
            }
        }

        [Test]
        [Row(null, null)]
        [Row(150, null)]
        [Row(null, 150)]
        [Row(300, 150)]
        public void Should_not_loose_messages(ushort? delayBetweenPublishesInMilliseconds, ushort? delayWhenStoringMessage)
        {
            using(storage = new FakeMessageStore(delayWhenStoringMessage))
                Run(delayBetweenPublishesInMilliseconds);
        }

        private void Run(ushort? delayBetweenPublishesInMilliseconds = null)
        {
            var storeTokens = MessageStore().ToArray();
            Start(() => Producer(delayBetweenPublishesInMilliseconds));

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
                var storeMessageQueue = model.QueueDeclare("", false, true, false, null);
                var liveMessageQueue = model.QueueDeclare("", false, false, false, null);
                
                var storeConsumer = new QueueingBasicConsumer(model);
                model.BasicConsume(storeMessageQueue, true, storeConsumer);

                var liveConsumer = new QueueingBasicConsumer(model);
                model.BasicConsume(liveMessageQueue, true, liveConsumer);

                var properties = model.CreateBasicProperties();
                properties.ReplyTo = storeMessageQueue;
                properties.Headers = new Hashtable { {LiveQueueHeader, liveMessageQueue.Bytes()} };

                model.BasicPublish("", ReplayerQueue, properties, new byte[0]);

                var lastMessageId = default(Guid);

                foreach (var message in Consume(storeConsumer.Queue))
                {
                    lastMessageId = GetId(message);

                    yield return message.Body.Integer();
                }

                foreach (var message in Consume(liveConsumer.Queue, lastMessageId))
                    yield return message.Body.Integer();

                Debug.WriteLine("Consumer: no message arrived in timely fashion, exiting");
            }
        }

        private Guid GetId(BasicDeliverEventArgs message)
        {
            return message.Id();
        }

        private static IEnumerable<BasicDeliverEventArgs> Consume(SharedQueue queue, Guid startAfterReceivingId = default(Guid))
        {
            object message;
            bool shouldConsume = false;

            while (queue.Dequeue(2000, out message))
            {
                var args = (BasicDeliverEventArgs)message;

                if(!shouldConsume && startAfterReceivingId != default(Guid))
                {
                    var messageId = args.Id();

                    if (messageId.Equals(startAfterReceivingId))
                        shouldConsume = true;

                    continue;
                }

                var intMessage = args.Body.Integer();

                Debug.WriteLine("Consumer: received message " + intMessage + " from "
                    + ((byte[])args.BasicProperties.Headers[SourceHeader]).String());

                yield return args;
            }
        }

        private void Producer(ushort? delayBetweenPublishesInMilliseconds = null)
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
                    properties.Headers = new Hashtable {{SourceHeader, "Producer".Bytes()}, { MessageExtensions.MessageIdHeader, Guid.NewGuid().ToString().Bytes() }};

                    model.BasicPublish(Exchange, "", properties, i.Bytes());
                    
                    if(delayBetweenPublishesInMilliseconds.HasValue)
                        Thread.Sleep(delayBetweenPublishesInMilliseconds.Value);
                }
            }

            Debug.WriteLine("Publisher: completed publishing");
        }

        private IEnumerable<CancellationTokenSource> MessageStore()
        {
            yield return Start(Store);
            yield return Start(Replayer);
        }

        private void Store()
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
                int receivedMessages = 0;

                foreach (BasicDeliverEventArgs message in consumer.Queue)
                {
                    Debug.WriteLine("Store: received new message to store");
                    receivedMessages++;

                    storage.Store(message);

                    if (receivedMessages >= messageNumber / 2)
                        storeHalfWay.Set();
                }
            }
        }

        private void Replayer()
        {
            using (var connection = Helpers.CreateConnection())
            using (var model = connection.CreateModel())
            {
                var incomingRequestsQueue = model.QueueDeclare(ReplayerQueue, false, false, true, null);

                var incomingRequestsConsumer = new QueueingBasicConsumer(model);
                model.BasicConsume(incomingRequestsQueue, true, incomingRequestsConsumer);

                replayerReady.Set();

                foreach (BasicDeliverEventArgs incomingRequest in incomingRequestsConsumer.Queue)
                {
                    Debug.WriteLine(string.Format("Replayer: received new client request, sending {0} messages", storage.Count));
                    model.QueueBind(((byte[])incomingRequest.BasicProperties.Headers[LiveQueueHeader]).String(), Exchange, "");

                    // this is ugly but how do I know when the client has started receiving messages
                    // and thus I can start to send mine so that at least one overlaps?
                    Thread.Sleep(500);

                    foreach (var message in storage)
                    {
                        var properties = message.BasicProperties;
                        properties.Headers[SourceHeader] = "Store".Bytes();

                        model.BasicPublish("", incomingRequest.BasicProperties.ReplyTo, properties, message.Body);
                    } 
                }
            }
        }
    }
}