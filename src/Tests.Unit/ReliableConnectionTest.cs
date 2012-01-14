using System;
using System.Threading;
using System.Threading.Tasks;
using MbUnit.Framework;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Roger;
using Roger.Internal;
using Roger.Internal.Impl;

namespace Tests.Unit
{
    [TestFixture]
    public class ReliableConnectionTest
    {
        private IConnectionFactory connectionFactory;
        private ReliableConnection sut;
        private ITimer timer;

        [SetUp]
        public void Setup()
        {
            connectionFactory = Substitute.For<IConnectionFactory>();
            timer = Substitute.For<ITimer>();

            sut = new ReliableConnection(connectionFactory, timer);
        }

        [Test]
        public void Should_call_factory_when_asked_to_connect()
        {
            sut.Connect();

            connectionFactory.Received().CreateConnection();
        }

        [Test]
        public void Should_block_until_first_connection_attempt_completes_successfully()
        {
            RunTest(() => {});
        }

        [Test]
        public void Should_block_until_first_connection_attempt_completes_with_a_failure()
        {
            RunTest(() => { throw new BrokerUnreachableException(null, null); });
        }

        [Test]
        public void Should_schedule_reconnection_when_connection_attempt_fails()
        {
            connectionFactory.CreateConnection().Returns(_ => { throw new BrokerUnreachableException(null, null); });
            sut.Connect();
            timer.Received().Start(sut.ConnectionAttemptInterval);
        }

        [Test]
        public void Should_reconnect_when_connection_shuts_down()
        {
            var connection = Substitute.For<IConnection>();
            connectionFactory.CreateConnection().Returns(connection);

            sut.Connect();

            connection.ConnectionShutdown +=
                Raise.Event<ConnectionShutdownEventHandler>(connection, new ShutdownEventArgs(ShutdownInitiator.Application, 0, ""));

            timer.Received().Start(sut.ConnectionAttemptInterval);
        }

        [Test]
        public void Should_not_reconnect_on_connection_shutdown_if_it_has_been_disposed_of()
        {
            var connection = Substitute.For<IConnection>();
            connectionFactory.CreateConnection().Returns(connection);

            sut.Connect();
            sut.Dispose();

            connection.ConnectionShutdown +=
                Raise.Event<ConnectionShutdownEventHandler>(connection, new ShutdownEventArgs(ShutdownInitiator.Application, 0, ""));

            timer.DidNotReceiveWithAnyArgs().Start(TimeSpan.Zero);
        }

        [Test]
        public void Should_stop_timer_on_dispose()
        {
            sut.Connect();
            sut.Dispose();

            timer.Received().Stop();
        }

        private void RunTest(Action createConnection)
        {
            var currentThread = Thread.CurrentThread.ManagedThreadId;
            var createConnectionThread = 0;
            var createConnectionCalled = new ManualResetEvent(false);

            connectionFactory.When(f => f.CreateConnection()).Do(_ =>
            {
                createConnectionThread = Thread.CurrentThread.ManagedThreadId;
                createConnection();
                createConnectionCalled.Set();
            });

            sut.Connect();
            createConnectionCalled.WaitOne(100);
            Assert.AreEqual(currentThread, createConnectionThread);
        }
    }
}