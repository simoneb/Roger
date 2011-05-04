using ZMQ;

namespace ZeroMQExtensions
{
    public static class ContextExtensions
    {
        public static IPubSocket Pub(this Context context)
        {
            return new PubSocket(context.Socket(SocketType.PUB));
        }

        public static ISubSocket Sub(this Context context)
        {
            return new SubSocket(context.Socket(SocketType.SUB));
        }

        public static IPullSocket Pull(this Context context)
        {
            return new PullSocket(context.Socket(SocketType.PULL));
        }

        public static IPushSocket Push(this Context context)
        {
            return new PushSocket(context.Socket(SocketType.PUSH));
        }
    }
}