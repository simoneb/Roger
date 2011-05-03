using ZMQ;

namespace ZeroMQExtensions
{
    public static class ContextExtensions
    {
        public static Socket Pub(this Context context)
        {
            return context.Socket(SocketType.PUB);
        }

        public static Socket Sub(this Context context)
        {
            return context.Socket(SocketType.SUB);
        }
    }
}