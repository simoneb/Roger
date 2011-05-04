using ZMQ;

namespace ZeroMQExtensions
{
    internal class PubSocket : SocketImpl, IPubSocket
    {
        internal PubSocket(Socket socket) : base(socket)
        {
        }
    }
}