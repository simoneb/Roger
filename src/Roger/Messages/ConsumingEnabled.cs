using Roger.Internal;

namespace Roger.Messages
{
    internal class ConsumingEnabled
    {
        public RogerEndpoint Endpoint { get; private set; }

        public IReliableConnection Connection { get; private set; }

        public ConsumingEnabled(RogerEndpoint endpoint, IReliableConnection connection)
        {
            Endpoint = endpoint;
            Connection = connection;
        }
    }
}