using ZMQ;

namespace ZeroMQExtensions
{
    internal class PushSocket : SocketImpl, IPushSocket
    {
        public PushSocket(Socket socket) : base(socket)
        {
        }
    }
}