using System;
using System.Collections.Generic;
using System.Reflection;

namespace Roger.Internal.Impl
{
    internal class DefaultReflection : IReflection
    {
        private static readonly string ConsumeMethodName = typeof(IConsumer<>).GetMethod("Consume").Name;

        private static readonly MethodInfo InternalPreserveStackTraceMethod =
            typeof(Exception).GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic);

        public void InvokeConsume(IConsumer consumer, object message)
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

        public IEnumerable<Type> Hierarchy(Type type)
        {
            var stack = new Stack<Type>();

            var @base = type;

            while (@base != null && @base != typeof(object))
            {
                stack.Push(@base);
                @base = @base.BaseType;
            }

            return stack;
        }

        private static Exception InnerExceptionWhilePreservingStackTrace(TargetInvocationException e)
        {
            InternalPreserveStackTraceMethod.Invoke(e.InnerException, new object[0]);
            return e.InnerException;
        }
    }
}