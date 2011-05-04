using System.Text;
using ZMQ;

namespace ZeroMQExtensions
{
    public static class SocketExtensions
    {
        public static T BoundTo<T>(this T socket, string address) where T : ISocket
        {
            socket.Bind(address);
            return socket;
        }

        public static T ConnectedTo<T>(this T socket, string address) where T : ISocket
        {
            socket.Connect(address);
            return socket;
        }

        public static ISubSocket SubscribedTo(this ISubSocket socket, string filter, Encoding encoding)
        {
            socket.Subscribe(filter, encoding);
            return socket;
        }

        public static ISubSocket SubscribedToAnything(this ISubSocket socket)
        {
            socket.Subscribe(new byte[0]);
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

        public static T WithIdentity<T>(this T socket, string identity, Encoding encoding) where T : ISocket
        {
            socket.StringToIdentity(identity, encoding);
            return socket;
        }

        public static void Dump(this ISocket socket, Encoding encoding)
        {
            ZHelpers.Dump(socket.Socket, encoding);
        }
    }
}
