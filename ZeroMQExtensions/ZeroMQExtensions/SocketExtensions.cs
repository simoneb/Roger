using System.Text;
using ZMQ;

namespace ZeroMQExtensions
{
    public static class SocketExtensions
    {
        public static Socket BoundTo(this Socket socket, string address)
        {
            socket.Bind(address);
            return socket;
        }

        public static Socket ConnectedTo(this Socket socket, string address)
        {
            socket.Connect(address);
            return socket;
        }

        public static Socket SubscribedTo(this Socket socket, string filter, Encoding encoding)
        {
            socket.Subscribe(filter, encoding);
            return socket;
        }
    }
}
