using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Integration.Utils
{
    public static class Extensions
    {
         public static void Times(this int times, Action<int> action)
         {
             for (int i = 0; i < times; i++)
                 action(i);
         }

         public static IEnumerable<T> Times<T>(this int times, IEnumerable<T> enumerable)
         {
             IEnumerable<T> result = enumerable;

             for (int i = 1; i < times; i++)
                 result = result.Concat(enumerable);

             return result;
         }

        public static TimeSpan Seconds(this int number)
        {
            return TimeSpan.FromSeconds(number);
        }
    }
}