namespace Rabbus.Reflection
{
    public interface IReflection
    {
        void InvokeConsume(IConsumer consumer, object message);
    }
}