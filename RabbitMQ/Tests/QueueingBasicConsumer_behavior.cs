using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using Common;
using MbUnit.Framework;
using RabbitMQ.Client;
using System;

namespace Tests
{
    public abstract class QueueingBasicConsumer_behavior : With_rabbitmq_broker, IObserver<object>
    {
        protected IModel Model;
        private object message;
        private EventWaitHandle completionSemaphore;
        private EventWaitHandle errorSemaphore;
        protected IConnection Connection;
        protected QueueingBasicConsumer Consumer;
        private Exception exception;
        protected object InitialMessageValue;

        [SetUp]
        public void Setup()
        {
            Connection = Helpers.CreateConnection();
            Model = Connection.CreateModel();
            InitialMessageValue = new object();
            message = InitialMessageValue;
            completionSemaphore = new ManualResetEvent(false);
            errorSemaphore = new ManualResetEvent(false);
            exception = null;

            Consumer = new QueueingBasicConsumer(Model);
            Model.BasicConsume(Model.QueueDeclare(), true, Consumer);
            
            Consume.ToObservable(Scheduler.TaskPool).Subscribe(this);
        }

        [TearDown]
        public void TearDown()
        {
            if(Connection.IsOpen)
                Connection.Dispose();
        }

        protected object WaitForCompletion
        {
            get
            {
                completionSemaphore.WaitOne(1000);
                return message;
            }
        }

        protected Exception WaitForError
        {
            get
            {
                errorSemaphore.WaitOne(1000);
                return exception;
            }
        }

        protected abstract IEnumerable<object> Consume { get; }

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