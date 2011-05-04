using System.Text;
using ZMQ;

namespace ZeroMQExtensions
{
    internal class SubSocket : SocketImpl, ISubSocket
    {
        internal SubSocket(Socket socket) : base(socket)
        {
        }

        public void Subscribe(byte[] filter)
        {
            Socket.Subscribe(filter);
        }

        public void Subscribe(string filter, Encoding encoding)
        {
            Socket.Subscribe(filter, encoding);
        }

        public void Unsubscribe(byte[] filter)
        {
            Socket.Unsubscribe(filter);
        }

        public void Unsubscribe(string filter, Encoding encoding)
        {
            Socket.Unsubscribe(filter, encoding);
        }

    }
}