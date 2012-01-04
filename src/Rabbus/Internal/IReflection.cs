namespace Rabbus.Internal
{
    internal interface IReflection
    {
        void InvokeConsume(IConsumer consumer, object message);
    }
}