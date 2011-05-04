using ZMQ;

namespace ZeroMQExtensions
{
    internal class PullSocket : SocketImpl, IPullSocket
    {
        internal PullSocket(Socket socket) : base(socket)
        {
            
        }
    }
}