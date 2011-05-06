using System.Text;

namespace Common
{
    public static class Extensions
    {
        public static byte[] Bytes(this string @string)
        {
            return Encoding.UTF8.GetBytes(@string);
        }

        public static string String(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }
    }
}