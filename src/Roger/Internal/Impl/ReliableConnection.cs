using System;
using System.IO;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Roger.Internal.Impl
{
    internal class ReliableConnection : IReliableConnection
    {
        private readonly IConnectionFactory connectionFactory;
        private readonly IRogerLog log;
        public event Action ConnectionEstabilished = delegate {  };
        private IConnection connection;
        private bool disposed;
        private Timer initializationTimer;
        private static readonly TimeSpan Never = TimeSpan.FromMilliseconds(-1);
        public TimeSpan ConnectionAttemptInterval { get { return TimeSpan.FromSeconds(5); } }

        public event Action ConnectionAttemptFailed = delegate { };
        public event Action GracefulShutdown = delegate { };
        public event Action<ShutdownEventArgs> UnexpectedShutdown = delegate { };

        public ReliableConnection(IConnectionFactory connectionFactory, IRogerLog log)
        {
            this.connectionFactory = connectionFactory;
            this.log = log;
        }

        public void Connect()
        {
            try
            {
                connection = connectionFactory.CreateConnection();
            }
            catch (BrokerUnreachableException e) // looking at the client source it appears safe to catch this one only
            {
                log.ErrorFormat("Cannot create connection, broker is unreachable\r\n{0}", e);

                ConnectionAttemptFailed();

                ScheduleConnect();
                return;
            }

            log.Debug("Connection created");
            connection.ConnectionShutdown += HandleConnectionShutdown;
            ConnectionEstabilished();
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

        private void HandleConnectionShutdown(IConnection conn, ShutdownEventArgs reason)
        {
            // remove handler to be safe and prevent eventual callbacks from being invoked by closed connections
            conn.ConnectionShutdown -= HandleConnectionShutdown;

            // connection has been closed because we asked it!
            if (disposed)
            {
                GracefulShutdown();
                log.Debug("Connection has been shut down gracefully upon request");
            }
            else
            {
                UnexpectedShutdown(reason);
                log.DebugFormat("Connection (hashcode {0}) was shut down unexpectedly: {1}", conn.GetHashCode(), reason);

                ScheduleConnect();
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            if (connection != null)
                try
                {
                    connection.Close();
                }
                catch (AlreadyClosedException e)
                {
                    log.ErrorFormat("Trying to close connection but it was already closed.\r\n{0}", e);
                }
                catch (IOException e)
                {
                    log.ErrorFormat("Trying to close connection but something went wrong.\r\n{0}", e);
                }
                catch (Exception e)
                {
                    log.ErrorFormat("Trying to close connection but something went wrong.\r\n{0}", e);
                }
        }

        public IModel CreateModel()
        {
            return connection.CreateModel();
        }
    }
}