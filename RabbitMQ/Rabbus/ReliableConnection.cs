using System;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Rabbus.Logging;

namespace Rabbus
{
    internal class ReliableConnection : IReliableConnection
    {
        private readonly IConnectionFactory connectionFactory;
        private readonly IRabbusLog log;
        private readonly Action onSuccessfulConnection;
        private IConnection connection;
        private bool disposed;
        private Timer initializationTimer;
        private static readonly TimeSpan Never = TimeSpan.FromMilliseconds(-1);
        public TimeSpan ConnectionAttemptInterval { get { return TimeSpan.FromSeconds(5); } }

        public event Action ConnectionAttemptFailed = delegate { };
        public event Action GracefulShutdown = delegate { };
        public event Action<ShutdownEventArgs> UnexpectedShutdown = delegate { };

        public ReliableConnection(IConnectionFactory connectionFactory, IRabbusLog log, Action onSuccessfulConnection)
        {
            this.connectionFactory = connectionFactory;
            this.log = log;
            this.onSuccessfulConnection = onSuccessfulConnection;
        }

        public void Connect()
        {
            try
            {
                connection = connectionFactory.CreateConnection();
            }
            catch (BrokerUnreachableException e)
            {
                log.ErrorFormat("Cannot create connection, broker is unreachable, rescheduling.\r\n{0}", e);

                ConnectionAttemptFailed();

                ScheduleConnect();
                return;
            }

            log.Debug("Connection created");
            connection.ConnectionShutdown += HandleConnectionShutdown;
            onSuccessfulConnection();
        }

        private void HandleConnectionShutdown(IConnection conn, ShutdownEventArgs reason)
        {
            // remove handler to be safe and prevent eventual callbacks from being invoked by closed connections
            conn.ConnectionShutdown -= HandleConnectionShutdown;

            // connection has been closed because we asked it!
            if (disposed)
            {
                GracefulShutdown();
                log.Debug("Connection has been shut down");
            }
            else
            {
                UnexpectedShutdown(reason);
                log.DebugFormat("Connection (hashcode {0}) was shut down unexpectedly, rescheduling: {1}", conn.GetHashCode(), reason);

                ScheduleConnect();
            }
        }

        private void ScheduleConnect()
        {
            log.DebugFormat("Scheduling connection to be retried in {0}", ConnectionAttemptInterval);

            initializationTimer = new Timer(timer =>
            {
                ((Timer)timer).Dispose();

                if (!disposed)
                    Connect();
            });

            initializationTimer.Change(ConnectionAttemptInterval, Never);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            if (connection != null)
                try
                {
                    // here we dispose just the connection because the client library user guide
                    // says that doing so all channels are implicitly closed as well
                    connection.Dispose();
                }
                catch (AlreadyClosedException e)
                {
                    log.ErrorFormat("Trying to close connection but it was already closed.\r\n{0}", e);
                }
        }

        public IModel CreateModel()
        {
            return connection.CreateModel();
        }
    }
}