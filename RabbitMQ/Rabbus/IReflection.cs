namespace Rabbus
{
    public interface IReflection
    {
        void InvokeConsume(IConsumer consumer, object message);
    }
}