using System;
using System.IO;
using System.Threading;
using Common.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Roger.Messages;

namespace Roger.Internal.Impl
{
    internal class ReliableConnection : IReliableConnection
    {
        private readonly IConnectionFactory connectionFactory;
        private readonly ITimer timer;
        private readonly IWaiter waiter;
        private readonly IAggregator aggregator;
        private readonly ILog log = LogManager.GetCurrentClassLogger();
        private IConnection connection;
        private int disposed;
        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
        private CancellationToken token;

        public TimeSpan ConnectionAttemptInterval { get { return TimeSpan.FromSeconds(5); } }

        public ReliableConnection(IConnectionFactory connectionFactory, ITimer reconnectionTimer, IAggregator aggregator) : this(connectionFactory, reconnectionTimer, new DefaultWaiter(), aggregator)
        {
        }

        public ReliableConnection(IConnectionFactory connectionFactory, ITimer timer, IWaiter waiter, IAggregator aggregator)
        {
            this.connectionFactory = connectionFactory;
            this.timer = timer;
            this.waiter = waiter;
            this.aggregator = aggregator;
            token = tokenSource.Token;
            timer.Callback += BlockingConnect;
        }

        public void Connect()
        {
            BlockingConnect();
        }

        private void BlockingConnect()
        {
            while (!token.IsCancellationRequested)
                try
                {
                    log.Debug("Trying to connect to broker");
                    connection = connectionFactory.CreateConnection();
                    
                    log.Debug("Connection created");
                    connection.ConnectionShutdown += HandleConnectionShutdown;

                    aggregator.Notify(new ConnectionEstablished(this));
                    return;
                }
                catch (BrokerUnreachableException e) // looking at the client source it appears safe to catch this exception only
                {
                    log.Error("Cannot create connection, broker is unreachable", e);

                    aggregator.Notify(new ConnectionAttemptFailed());

                    if (waiter.Wait(token.WaitHandle, ConnectionAttemptInterval))
                        return;
                }
        }

        private void HandleConnectionShutdown(IConnection conn, ShutdownEventArgs reason)
        {
            // remove handler to be safe and prevent eventual callbacks from being invoked by closed connections
            conn.ConnectionShutdown -= HandleConnectionShutdown;

            // connection has been closed because we asked it!
            if (disposed == 1)
            {
                aggregator.Notify(new GracefulConnectionShutdown());
                log.Debug("Connection has been shut down gracefully upon request");
            }
            else
            {
                aggregator.Notify(new ConnectionUnexpectedShutdown(reason));
                log.DebugFormat("Connection (hashcode {0}) was shut down unexpectedly: {1}", conn.GetHashCode(), reason);

                log.DebugFormat("Scheduling connection to be retried in {0}", ConnectionAttemptInterval);

                timer.Start(ConnectionAttemptInterval);
            }
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref disposed, 1, 0) == 1)
                return;

            timer.Stop();
            tokenSource.Cancel();

            if (connection != null)
                try
                {
                    connection.Close();
                }
                catch (AlreadyClosedException e)
                {
                    log.Error("Trying to close connection but it was already closed", e);
                }
                catch (IOException e)
                {
                    log.Error("Trying to close connection but something went wrong", e);
                }
                catch (Exception e)
                {
                    log.Error("Trying to close connection but something went wrong", e);
                }
        }

        public IModel CreateModel()
        {
            return connection.CreateModel();
        }
    }
}