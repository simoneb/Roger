using System.Collections.Generic;

namespace Rabbus.Utilities
{
    public static class EnumerableExtensions
    {
         public static IEnumerable<T> Return<T>(this T value)
         {
             yield return value;
         }
    }
}