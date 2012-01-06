namespace Roger.Internal
{
    internal interface IReflection
    {
        void InvokeConsume(IConsumer consumer, object message);
    }
}