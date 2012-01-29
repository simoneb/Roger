using System;
using System.Collections.Generic;
using System.Threading;
using Common;
using Common.Logging;
using MbUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Tests.Integration.Exploratory.Utils;

namespace Tests.Integration.Exploratory
{
    public class Shutdown_protocol : With_rabbitmq_broker
    {
        private readonly ILog log = LogManager.GetCurrentClassLogger();
        private IConnection connection;
        private WhatHappens wh;

        [SetUp]
        public void Setup()
        {
            connection = Helpers.CreateConnection();
            wh = new WhatHappens();
        }

        public class Channel_closure_causes : Shutdown_protocol
        {
            [Test]
            public void queueDeclarePassive_when_queue_does_not_exist_throws_OIE()
            {
                var model = connection.CreateModel();

                Assert.Throws<OperationInterruptedException>(() => model.QueueDeclarePassive(Guid.NewGuid().ToString()));
            }

            [Test]
            public void queueDeclarePassive_when_queue_does_not_exist_closes_the_channel()
            {
                var model = connection.CreateModel();

                try
                {
                    model.QueueDeclarePassive(Guid.NewGuid().ToString());
                }
                catch { }

                Assert.IsFalse(model.IsOpen);

                // in current client library AlreadyClosedException is swallowed and OIE is let bubble up
                Assert.Throws<OperationInterruptedException>(() => model.QueueDeclare("", false, true, true, null));
            }

            [Test]
            public void Publishing_to_an_exchange_which_does_not_exist_does_not_throw()
            {
                var model = connection.CreateModel();

                Assert.DoesNotThrow(() => model.BasicPublish("unexisting", "whatherver", null, "Ciao".Bytes()));
            }

            [Test]
            public void Publishing_to_an_exchange_which_does_not_exist_then_trying_to_use_the_model_will_throw_the_exception_related_to_previous_error()
            {
                var model = connection.CreateModel();

                model.BasicPublish("unexisting", "whatherver", null, "Ciao".Bytes());

                var exception = Assert.Throws<OperationInterruptedException>(() => model.QueueDeclare("", false, true, false, null));
                Assert.AreEqual(404, exception.ShutdownReason.ReplyCode);
                Assert.Contains(exception.ShutdownReason.ReplyText, "unexisting");
            }

            [Test]
            public void Publishing_to_an_exchange_which_does_not_exist_then_trying_to_use_the_model_find_the_model_closed()
            {
                var model = connection.CreateModel();

                model.BasicPublish("unexisting", "whatherver", null, "Ciao".Bytes());

                try
                {
                    model.QueueDeclare("", false, true, false, null);
                }
                catch{ }

                Assert.IsFalse(model.IsOpen);
            }
        }

        public class When_connection_closes : Shutdown_protocol
        {
            public IEnumerable<Action<IConnection>> CloseConnection()
            {
                yield return c => c.Close();
                yield return c => c.Dispose();
                yield return c => c.Abort();
            }

            [Test, Factory("CloseConnection")]
            public void Should_notify_connection_and_then_model(Action<IConnection> close)
            {
                connection.ConnectionShutdown += wh.CaptureEvent<ConnectionShutdownEventHandler>();

                var model = connection.CreateModel();

                model.ModelShutdown += wh.CaptureEvent<ModelShutdownEventHandler>();

                close(connection);
            }

            private class CustomConsumer : QueueingBasicConsumer
            {
                private readonly WhatHappens whatHappens;

                public CustomConsumer(WhatHappens whatHappens)
                {
                    this.whatHappens = whatHappens;
                }

                public override void HandleBasicCancel(string consumerTag)
                {
                    whatHappens.Act(new MethodCallExpectation(this, typeof(CustomConsumer).GetMethod("HandleBasicCancel")));
                    base.HandleBasicCancel(consumerTag);
                }
            }

            [TearDown]
            public void TearDown()
            {
                wh.Verify();
            }
        }

        public class When_model_closes : Shutdown_protocol
        {
            public IEnumerable<Action<IModel>> CloseModel()
            {
                yield return m => m.Close();
                yield return m => m.Dispose();
                yield return m => m.Abort();
            }

            [Test, Factory("CloseModel")]
            public void Should_notify_model_and_then_consumer(Action<IModel> close)
            {
                var model = connection.CreateModel();

                var consumer = new SpyConsumer(wh, cancel: true, shutdown: true);

                model.BasicConsume(model.QueueDeclare(), false, consumer);

                wh.CaptureMethod<string>(consumer.HandleBasicCancel);
                wh.CaptureMethod<IModel, ShutdownEventArgs>(consumer.HandleModelShutdown);
                model.ModelShutdown += wh.CaptureEvent<ModelShutdownEventHandler>();

                close(model);
            }

            private class SpyConsumer : QueueingBasicConsumer
            {
                private readonly WhatHappens whatHappens;
                private readonly bool cancel;
                private readonly bool shutdown;

                public SpyConsumer(WhatHappens whatHappens, bool cancel = false, bool shutdown = false)
                {
                    this.whatHappens = whatHappens;
                    this.cancel = cancel;
                    this.shutdown = shutdown;
                }

                public override void HandleBasicCancel(string consumerTag)
                {
                    if(cancel)
                        whatHappens.Act(new MethodCallExpectation(this, typeof(SpyConsumer).GetMethod("HandleBasicCancel")));

                    base.HandleBasicCancel(consumerTag);
                }

                public override void HandleModelShutdown(IModel model, ShutdownEventArgs reason)
                {
                    if(shutdown)
                        whatHappens.Act(new MethodCallExpectation(this, typeof(SpyConsumer).GetMethod("HandleModelShutdown")));

                    base.HandleModelShutdown(model, reason);
                }
            }

            [TearDown]
            public void TearDown()
            {
                wh.Verify();
            }
        }

        [Test]
        [Explicit]
        public void ManualTest()
        {

            connection.CallbackException += LoggingConnectionCallbackException(1);
            //connection.CallbackException += ThrowingConnectionCallbackException;
            //connection.CallbackException += LoggingConnectionCallbackException(2);
            connection.ConnectionShutdown += LoggingConnectionOnConnectionShutdown(1);
            //connection.ConnectionShutdown += ThrowingConnectionOnConnectionShutdown;
            //connection.ConnectionShutdown += LoggingConnectionOnConnectionShutdown(2);

            var model = connection.CreateModel();
            //model.TxSelect();
            //model.ConfirmSelect();

            model.CallbackException += LoggingModelOnCallbackException(1);
            //model.CallbackException += ThrowingModelOnCallbackException;
            //model.CallbackException += LoggingModelOnCallbackException(2);
            model.ModelShutdown += LoggingModelOnModelShutdown(1);
            //model.ModelShutdown += ThrowingModelOnModelShutdown;
            //model.ModelShutdown += LoggingModelOnModelShutdown(2);

            var consumer = new SpyConsumer();
            var queue = model.QueueDeclare("", false, true, true, null);

            log.Debug("Invoking basic consume");
            model.BasicConsume(queue, true, consumer);

            log.Debug("Invoking basic publish");
            model.BasicPublish("", queue, null, "Ciao".Bytes());

            Thread.Sleep(2000);

            //model.BasicAck(2, false);

            Thread.Sleep(2000);
            model.QueueDelete(queue);

            Thread.Sleep(2000);

            log.Debug("Closing model");
            model.Close();

            Thread.Sleep(2000);


            //log.Debug("Closing connection");
            //connection.Close();
            
        }

        private ModelShutdownEventHandler LoggingModelOnModelShutdown(int n)
        {
            return (m, r) => log.DebugFormat("Model shutdown handler {0}: {1}", n, r);
        }

        private CallbackExceptionEventHandler LoggingModelOnCallbackException(int n)
        {
            return (s, e) => log.DebugFormat("Model callback exception handler {0}: {1}", n, e.Detail["context"]);
        }

        private void ThrowingModelOnModelShutdown(IModel model, ShutdownEventArgs reason)
        {
            log.Debug("Throwing exception in model shutdown handler");
            throw new Exception();
        }

        private void ThrowingModelOnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            log.Debug("Throwing exception in model callback exception handler");
            throw new Exception();
        }

        private ConnectionShutdownEventHandler LoggingConnectionOnConnectionShutdown(int n)
        {
            return (c, e) => log.DebugFormat("Connection shutdown handler {0}: {1}", n, e);
        }

        private void ThrowingConnectionOnConnectionShutdown(IConnection connection, ShutdownEventArgs reason)
        {
            log.Debug("Throwing exception in connection shutdown handler");
            throw new Exception();
        }

        private CallbackExceptionEventHandler LoggingConnectionCallbackException(int n)
        {
            return (s, e) => log.DebugFormat("Connection callback exception handler {0}: {1}", n, e.Detail["context"]);
        }

        private void ThrowingConnectionCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            log.Debug("Throwing exception in connection callback exception handler");
            throw new Exception();
        }
    }

    public class SpyConsumer : DefaultBasicConsumer
    {
        private readonly ILog log = LogManager.GetCurrentClassLogger();

        public override void OnCancel()
        {
            log.Debug("OnCancel");
            base.OnCancel();
            //throw new Exception();
        }

        public override void HandleBasicConsumeOk(string consumerTag)
        {
            log.Debug("HandleBasicConsumeOk");
            base.HandleBasicConsumeOk(consumerTag);
            //throw new Exception();
        }

        public override void HandleBasicCancelOk(string consumerTag)
        {
            log.Debug("HandleBasicCancelOk");
            base.HandleBasicCancelOk(consumerTag);
            //throw new Exception();
        }

        public override void HandleBasicCancel(string consumerTag)
        {
            log.Debug("HandleBasicCancel");
            base.HandleBasicCancel(consumerTag);
            //throw new Exception();
        }

        public override void HandleModelShutdown(IModel model, ShutdownEventArgs reason)
        {
            log.DebugFormat("HandleModelShutdown: {0}", reason);
            base.HandleModelShutdown(model, reason);
            //throw new Exception();
        }

        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            log.Debug("HandleBasicDeliver");
            base.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);
            //throw new Exception();
        }
    }
}