using System.Reflection;

namespace Rabbus.Reflection
{
    public class DefaultReflection : IReflection
    {
        private static readonly string ConsumeMethodName = typeof (IConsumer<>).GetMethod("Consume").Name;

        public void InvokeConsume(IConsumer consumer, object message)
        {
            consumer.GetType().InvokeMember(ConsumeMethodName, BindingFlags.InvokeMethod, null, consumer, new[] { message });
        }
    }
}