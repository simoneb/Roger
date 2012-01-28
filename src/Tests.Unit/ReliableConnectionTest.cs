using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using MbUnit.Framework;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Roger;
using Roger.Internal;
using Roger.Internal.Impl;
using System.Linq;
using Roger.Messages;

namespace Tests.Unit
{
    [TestFixture]
    public class ReliableConnectionTest
    {
        private IConnectionFactory connectionFactory;
        private ReliableConnection sut;
        private ITimer timer;
        private IWaiter waiter;

        [SetUp]
        public void Setup()
        {
            connectionFactory = Substitute.For<IConnectionFactory>();
            timer = Substitute.For<ITimer>();
            waiter = Substitute.For<IWaiter>();

            sut = new ReliableConnection(connectionFactory, timer, waiter, new Aggregator());
        }

        [Test]
        public void Should_call_factory_when_asked_to_connect()
        {
            sut.Connect();

            connectionFactory.Received().CreateConnection();
        }

        [Test]
        public void Should_run_until_connection_succeeds_1()
        {
            var createConnectionCalled = new ManualResetEvent(false);

            connectionFactory.When(f => f.CreateConnection()).Do(_ => createConnectionCalled.Set());

            sut.Connect();
            Assert.IsTrue(createConnectionCalled.WaitOne());
        }

        [Test]
        public void Should_run_until_connection_succeeds_2()
        {
            var threads = new List<int>();

            var actions = new Queue<Action>(new[]
            {
                () => { throw BUE; }, 
                new Action(() => { })
            });

            connectionFactory.When(f => f.CreateConnection()).Do(_ =>
            {
                actions.Dequeue()();
                threads.Add(Thread.CurrentThread.ManagedThreadId);
            });

            sut.Connect();

            waiter.Received(1).Wait(Arg.Any<WaitHandle>(), sut.ConnectionAttemptInterval);
            Assert.AreEqual(0, actions.Count);
            Assert.AreEqual(1, threads.Distinct().Count());
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
        public void Should_stop_trying_to_connect_if_disposed_of()
        {
            connectionFactory.When(f => f.CreateConnection()).Do(_ => { sut.Dispose(); throw BUE; });
            waiter.Wait(Arg.Any<WaitHandle>(), sut.ConnectionAttemptInterval).Returns(true);

            sut.Connect();

            connectionFactory.Received(1).CreateConnection();
        }

        private static BrokerUnreachableException BUE
        {
            get { return new BrokerUnreachableException(new Hashtable(), new Hashtable()); }
        }

        [Test]
        public void Should_stop_timer_on_dispose()
        {
            sut.Connect();
            sut.Dispose();

            timer.Received().Stop();
        }
    }
}