using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public static class Extensions
    {
        public static byte[] Bytes(this int value)
        {
            return Bytes(value.ToString());
        }

        public static byte[] Bytes(this string @string)
        {
            return Encoding.UTF8.GetBytes(@string);
        }

        public static string String(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public static int Integer(this byte[] bytes)
        {
            return Int32.Parse(String(bytes));
        }

        public static IEnumerable<T> Times<T>(this int times, IEnumerable<T> enumerable)
        {
            var result = enumerable;

            for (int i = 1; i < times; i++)
                result = result.Concat(enumerable);

            return result;
        }
    }
}