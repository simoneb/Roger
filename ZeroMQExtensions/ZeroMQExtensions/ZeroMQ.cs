using System.Text;

namespace ZeroMQExtensions
{
    public static class ZeroMQ
    {
        static ZeroMQ()
        {
            DefaultEncoding = Encoding.UTF8;
        }

        public static Encoding DefaultEncoding { get; set; }
    }
}