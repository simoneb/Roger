using System;
using System.Linq;

namespace Rabbus
{
    public static class ReflectionExtensions
    {
        public static TAttribute Attribute<TAttribute>(this Type type)
        {
            return (TAttribute) type.GetCustomAttributes(typeof (TAttribute), true).Single();
        }

        public static string RoutingKey(this Type messageType)
        {
            return "";
        }
    }
}