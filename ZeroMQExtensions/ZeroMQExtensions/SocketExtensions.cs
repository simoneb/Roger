using ZMQ;

namespace ZeroMQExtensions
{
    public static class SocketExtensions
    {
        public static T Bind<T>(this T socket, string address) where T : ISocket
        {
            socket.Socket.Bind(address);
            return socket;
        }

        public static T Connect<T>(this T socket, string address) where T : ISocket
        {
            socket.Socket.Connect(address);
            return socket;
        }

        public static ISubSocket Subscribe(this ISubSocket socket, string filter)
        {
            socket.Socket.Subscribe(filter, ZeroMQ.DefaultEncoding);
            return socket;
        }

        public static ISubSocket SubscribeAll(this ISubSocket socket)
        {
            socket.Socket.Subscribe(new byte[0]);
            return socket;
        }

        public static T HighWatermark<T>(this T socket, ulong value) where T : ISocket
        {
            socket.HWM = value;
            return socket;
        }

        public static T Swapped<T>(this T socket, long value) where T : ISocket
        {
            socket.Swap = value;
            return socket;
        }

        public static T Identity<T>(this T socket, string identity) where T : ISocket
        {
            socket.Socket.StringToIdentity(identity, ZeroMQ.DefaultEncoding);
            return socket;
        }

        public static void Send(this ISocket socket, string message)
        {
            socket.Send(message, ZeroMQ.DefaultEncoding);
        }

        public static void Dump(this ISocket socket)
        {
            ZHelpers.Dump(socket.Socket, ZeroMQ.DefaultEncoding);
        }
    }
}
