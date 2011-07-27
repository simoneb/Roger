using System;
using System.Reflection;

namespace Rabbus.Reflection
{
    public class DefaultReflection : IReflection
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

        private static Exception InnerExceptionWhilePreservingStackTrace(TargetInvocationException e)
        {
            InternalPreserveStackTraceMethod.Invoke(e.InnerException, new object[0]);
            return e.InnerException;
        }
    }
}