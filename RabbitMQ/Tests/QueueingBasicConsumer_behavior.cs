using System.Collections.Generic;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using Common;
using MbUnit.Framework;
using RabbitMQ.Client;
using System;

namespace Tests
{
    public class QueueingBasicConsumer_behavior : With_rabbitmq_broker, IObserver<object>
    {
        private IModel model;
        private object message;
        private EventWaitHandle completionSemaphore;
        private EventWaitHandle errorSemaphore;
        private IConnection connection;
        private QueueingBasicConsumer consumer;
        private Exception exception;
        private object initialMessage;

        [SetUp]
        public void Setup()
        {
            connection = Helpers.CreateConnection();
            model = connection.CreateModel();
            initialMessage = new object();
            message = initialMessage;
            completionSemaphore = new ManualResetEvent(false);
            errorSemaphore = new ManualResetEvent(false);
            exception = null;

            consumer = new QueueingBasicConsumer(model);
            model.BasicConsume(model.QueueDeclare(), true, consumer);
            
            Consume.ToObservable(Scheduler.TaskPool).Subscribe(this);
        }

        [TearDown]
        public void TearDown()
        {
            if(connection.IsOpen)
                connection.Dispose();
        }

        [Test]
        public void When_model_closes_will_throw_and_queue_will_not_deliver_any_messages()
        {
            model.Dispose();

            Assert.IsInstanceOfType<EndOfStreamException>(WaitForError);

            Assert.AreSame(initialMessage, WaitForCompletion);
        }

        [Test]
        public void When_connection_closes_will_throw_and_queue_will_not_deliver_any_messages()
        {
            connection.Dispose();

            Assert.IsInstanceOfType<EndOfStreamException>(WaitForError);

            Assert.AreSame(initialMessage, WaitForCompletion);
        }

        private object WaitForCompletion
        {
            get
            {
                completionSemaphore.WaitOne(1000);
                return message;
            }
        }

        private Exception WaitForError
        {
            get
            {
                errorSemaphore.WaitOne(1000);
                return exception;
            }
        }

        private IEnumerable<object> Consume
        {
            get
            {
                while (true)
                    yield return consumer.Queue.Dequeue();
            }
        }

        public void OnNext(object value)
        {
            message = value;
        }

        public void OnError(Exception error)
        {
            exception = error;
            errorSemaphore.Set();
        }

        public void OnCompleted()
        {
            completionSemaphore.Set();
        }
    }
}