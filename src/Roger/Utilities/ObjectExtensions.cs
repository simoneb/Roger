namespace Rabbus.Utilities
{
    internal static class ObjectExtensions
    {
         public static T Or<T>(this T instance, T @this) where T : class
         {
             return instance ?? @this;
         }
    }
}