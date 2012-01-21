using System;
using System.Threading;
using Common;
using Common.Logging;
using MbUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Tests.Integration.Exploratory
{
    public class Shutdown_protocol : With_rabbitmq_broker
    {
        private ILog log = LogManager.GetCurrentClassLogger();

        [Test]
        public void test()
        {
            var connection = Helpers.CreateConnection();

            connection.CallbackException += LoggingConnectionCallbackException(1);
            connection.CallbackException += ThrowingConnectionCallbackException;
            connection.CallbackException += LoggingConnectionCallbackException(2);
            connection.ConnectionShutdown += LoggingConnectionOnConnectionShutdown(1);
            connection.ConnectionShutdown += ThrowingConnectionOnConnectionShutdown;
            connection.ConnectionShutdown += LoggingConnectionOnConnectionShutdown(2);

            var model = connection.CreateModel();

            model.CallbackException += LoggingModelOnCallbackException(1);
            model.CallbackException += ThrowingModelOnCallbackException;
            model.CallbackException += LoggingModelOnCallbackException(2);
            model.ModelShutdown += LoggingModelOnModelShutdown(1);
            model.ModelShutdown += ThrowingModelOnModelShutdown;
            model.ModelShutdown += LoggingModelOnModelShutdown(2);

            var consumer = new SpyConsumer();
            var queue = model.QueueDeclare("", false, true, true, null);

            log.Debug("Invoking basic consume");
            model.BasicConsume(queue, true, consumer);

            log.Debug("Invoking basic publish");
            model.BasicPublish("", queue, null, "Ciao".Bytes());

            Thread.Sleep(200);
           
            log.Debug("Closing connection");
            connection.Close();
            log.Debug("Closing model");
            model.Close();
        }

        private ModelShutdownEventHandler LoggingModelOnModelShutdown(int n)
        {
            return (m, r) => log.DebugFormat("Model shutdown {0}: {1}", n, r);
        }

        private CallbackExceptionEventHandler LoggingModelOnCallbackException(int n)
        {
            return (s, e) => log.DebugFormat("Model callback exception {0}: {1}", n, e.Detail["context"]);
        }

        private void ThrowingModelOnModelShutdown(IModel model, ShutdownEventArgs reason)
        {
            throw new Exception();
        }

        private void ThrowingModelOnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            throw new Exception();
        }

        private ConnectionShutdownEventHandler LoggingConnectionOnConnectionShutdown(int n)
        {
            return (c, e) => log.DebugFormat("Connection shutdown {0}: {1}", n, e);
        }

        private void ThrowingConnectionOnConnectionShutdown(IConnection connection, ShutdownEventArgs reason)
        {
            throw new Exception();
        }

        private CallbackExceptionEventHandler LoggingConnectionCallbackException(int n)
        {
            return (s, e) => log.DebugFormat("Connection callback exception {0}: {1}", n, e.Detail["context"]);
        }

        private void ThrowingConnectionCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            throw new Exception();
        }
    }

    public class SpyConsumer : DefaultBasicConsumer
    {
        private readonly ILog log = LogManager.GetCurrentClassLogger();

        public override void OnCancel()
        {
            log.Debug("OnCancel");
            throw new Exception();
        }

        public override void HandleBasicConsumeOk(string consumerTag)
        {
            log.Debug("HandleBasicConsumeOk");
            throw new Exception();
        }

        public override void HandleBasicCancelOk(string consumerTag)
        {
            log.Debug("HandleBasicCancelOk");
            throw new Exception();
        }

        public override void HandleBasicCancel(string consumerTag)
        {
            log.Debug("HandleBasicCancel");
            throw new Exception();
        }

        public override void HandleModelShutdown(IModel model, ShutdownEventArgs reason)
        {
            log.DebugFormat("HandleModelShutdown: {0}", reason);
            throw new Exception();
        }

        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            log.Debug("HandleBasicDeliver");
            throw new Exception();
        }
    }
}