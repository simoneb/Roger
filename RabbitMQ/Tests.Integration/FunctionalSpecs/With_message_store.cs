using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Common;
using MbUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Util;
using Tests.Integration.Utils;

namespace Tests.Integration.FunctionalSpecs
{
    public class With_message_store : With_rabbitmq_broker
    {
        protected const int messageNumber = 20;
        private const string Exchange = "MyMessageExchange";
        private const string ReplayerQueue = "MyMessageReplayerQueue";
        private const string LiveQueueHeader = "X-LiveQueue";
        private ManualResetEvent storeReady;
        private ManualResetEvent replayerReady;
        protected ManualResetEvent storeHalfWay;
        protected IMessageStore storage;

        [SetUp]
        public void InitializeHandles()
        {
            storeReady = new ManualResetEvent(false);
            replayerReady = new ManualResetEvent(false);
            storeHalfWay = new ManualResetEvent(false);
        }

        [TearDown]
        public void DeleteExchangeAndQueue()
        {
            using (var connection = Helpers.CreateConnection())
            using (var model = connection.CreateModel())
            {
                model.ExchangeDelete(Exchange);
                model.QueueDelete(ReplayerQueue);
            }
        }

        protected IEnumerable<BasicDeliverEventArgs> Consumer()
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
                properties.Headers = new Hashtable { {LiveQueueHeader, liveMessageQueue.QueueName.Bytes()} };

                model.BasicPublish("", ReplayerQueue, properties, new byte[0]);

                var lastMessageId = default(Guid);

                Debug.WriteLine("Consumer: consuming historical messages");

                foreach (var message in Consume(storeConsumer.Queue))
                {
                    lastMessageId = message.Id();

                    if(lastMessageId != Guid.Empty)
                        yield return message;
                }

                Debug.WriteLine("Consumer: consuming live messages");

                foreach (var message in Consume(liveConsumer.Queue, lastMessageId))
                    yield return message;
            }
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
                    if (args.Id().Equals(startAfterReceivingId))
                        shouldConsume = true;

                    continue;
                }

                try
                {
                    Debug.WriteLine("Consumer: received message " + args.Body.Integer() + " from " + args.Source());
                }
                catch (Exception e)
                {
                    
                }

                yield return args;
            }
        }

        protected void Producer(ushort? delayBetweenPublishesInMilliseconds = null)
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
                    properties.Headers = new Hashtable
                    {
                        {MessageExtensions.SourceHeader, "Producer".Bytes()},
                        {MessageExtensions.MessageIdHeader, Guid.NewGuid().ToString().Bytes()}
                    };

                    model.BasicPublish(Exchange, "", properties, i.Bytes());
                    
                    if(delayBetweenPublishesInMilliseconds.HasValue)
                        Thread.Sleep(delayBetweenPublishesInMilliseconds.Value);
                }
            }

            Debug.WriteLine("Publisher: completed publishing");
        }

        protected IEnumerable<CancellationTokenSource> MessageStore()
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
                    Debug.WriteLine("Replayer: received new client request");
                    model.QueueBind(((byte[])incomingRequest.BasicProperties.Headers[LiveQueueHeader]).String(), Exchange, "");

                    // this is ugly but how do I know when the client has started receiving messages
                    // and thus I can start to send mine so that at least one overlaps?
                    Thread.Sleep(500);

                    Debug.WriteLine("Replayer: sending messages from store");

                    var readFromStorage = false;

                    foreach (var message in storage)
                    {
                        readFromStorage = true;
                        var properties = message.BasicProperties;
                        properties.Headers[MessageExtensions.SourceHeader] = "Store".Bytes();

                        model.BasicPublish("", incomingRequest.BasicProperties.ReplyTo, properties, message.Body);
                    } 

                    if(!readFromStorage)
                    {
                        var basicProperties = model.CreateBasicProperties();
                        basicProperties.Headers = new Hashtable {{MessageExtensions.MessageIdHeader, Guid.Empty.ToString().Bytes()}};
                        model.BasicPublish("", incomingRequest.BasicProperties.ReplyTo, basicProperties, new byte[0]);
                    }
                }
            }
        }
    }
}