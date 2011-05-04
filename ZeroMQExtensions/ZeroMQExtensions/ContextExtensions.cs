using ZMQ;

namespace ZeroMQExtensions
{
    public static class ContextExtensions
    {
        public static ISocket Pub(this Context context)
        {
            return new SocketImpl(context.Socket(SocketType.PUB));
        }

        public static ISubSocket Sub(this Context context)
        {
            return new SubSocket(context.Socket(SocketType.SUB));
        }

        public static ISocket Pull(this Context context)
        {
            return new SocketImpl(context.Socket(SocketType.PULL));
        }

        public static ISocket Push(this Context context)
        {
            return new SocketImpl(context.Socket(SocketType.PUSH));
        }

        public static ISocket Router(this Context context)
        {
            return new SocketImpl(context.Socket(SocketType.XREP));
        }

        public static ISocket Req(this Context context)
        {
            return new SocketImpl(context.Socket(SocketType.REQ));
        }

        public static ISocket Rep(this Context context)
        {
            return new SocketImpl(context.Socket(SocketType.REP));
        }

        public static ISubSocket Sub(this Context context, string identity)
        {
            return Sub(context).Identity(identity);
        }

        public static ISocket Req(this Context context, string identity)
        {
            return Req(context).Identity(identity);
        }
    }
}