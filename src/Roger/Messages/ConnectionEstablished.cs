using Roger.Internal;

namespace Roger.Messages
{
    internal class ConnectionEstablished
    {
        public ConnectionEstablished(IReliableConnection connection)
        {
            Connection = connection;
        }

        public IReliableConnection Connection { get; private set; }
    }
}