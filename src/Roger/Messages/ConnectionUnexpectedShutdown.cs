using RabbitMQ.Client;

namespace Roger.Messages
{
    internal class ConnectionUnexpectedShutdown
    {
        public ShutdownEventArgs Reason { get; private set; }

        public ConnectionUnexpectedShutdown(ShutdownEventArgs reason)
        {
            Reason = reason;
        }
    }
}