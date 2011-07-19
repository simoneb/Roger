using System;
using System.Linq;

namespace Rabbus.Reflection
{
    public static class ReflectionExtensions
    {
        public static TAttribute Attribute<TAttribute>(this Type type)
        {
            return (TAttribute) type.GetCustomAttributes(typeof (TAttribute), true).Single();
        }
    }
}