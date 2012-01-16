using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Roger.Internal.Impl
{
    internal static class ReflectionExtensions
    {
        private static readonly string ConsumeMethodName = typeof(IConsumer<>).GetMethod("Consume").Name;

        private static readonly MethodInfo InternalPreserveStackTraceMethod =
            typeof(Exception).GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic);

        public static Type HierarchyRoot(this Type type)
        {
            return type.Hierarchy().Last();
        }

        public static IEnumerable<Type> Hierarchy(this Type type)
        {
            var @base = type;

            while (@base != null && @base != typeof(object))
            {
                yield return @base;
                @base = @base.BaseType;
            }
        }

        public static void InvokePreservingStackTrace(this IConsumer consumer, object message)
        {
            try
            {
                consumer.GetType().InvokeMember(ConsumeMethodName, BindingFlags.InvokeMethod, null, consumer, new[] { message });
            }
            catch (TargetInvocationException e)
            {
                throw InnerExceptionWhilePreservingStackTrace(e);
            }
        }

        private static Exception InnerExceptionWhilePreservingStackTrace(TargetInvocationException e)
        {
            InternalPreserveStackTraceMethod.Invoke(e.InnerException, new object[0]);
            return e.InnerException;
        }

        internal static Type ConsumerOf(this Type messageType)
        {
            return typeof(IConsumer<>).MakeGenericType(messageType);
        }
    }
}