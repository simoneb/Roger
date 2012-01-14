using System;
using System.IO;
using Common.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Roger.Internal.Impl
{
    internal class ReliableConnection : IReliableConnection
    {
        private readonly IConnectionFactory connectionFactory;
        private readonly ITimer timer;
        private readonly ILog log = LogManager.GetCurrentClassLogger();
        private IConnection connection;
        private bool disposed;

        public TimeSpan ConnectionAttemptInterval { get { return TimeSpan.FromSeconds(5); } }

        public event Action ConnectionEstabilished = delegate {  };
        public event Action ConnectionAttemptFailed = delegate { };
        public event Action GracefulShutdown = delegate { };
        public event Action<ShutdownEventArgs> UnexpectedShutdown = delegate { };

        public ReliableConnection(IConnectionFactory connectionFactory, ITimer timer)
        {
            this.connectionFactory = connectionFactory;
            this.timer = timer;
            timer.Callback += Connect;
        }

        public void Connect()
        {
            try
            {
                log.Debug("Trying to connect to broker");
                connection = connectionFactory.CreateConnection();
            }
            catch (BrokerUnreachableException e) // looking at the client source it appears safe to catch this exception only
            {
                log.Error("Cannot create connection, broker is unreachable", e);

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

            timer.Start(ConnectionAttemptInterval);
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

            timer.Stop();

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