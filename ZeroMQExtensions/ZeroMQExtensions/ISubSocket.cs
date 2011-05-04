using System.Text;

namespace ZeroMQExtensions
{
    public interface ISubSocket : ISocket
    {
        void Subscribe(byte[] filter);
        void Subscribe(string filter, Encoding encoding);
        void Unsubscribe(byte[] filter);
        void Unsubscribe(string filter, Encoding encoding);
    }
}