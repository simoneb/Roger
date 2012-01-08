namespace Roger
{
    public interface IConsumer<in T> : IConsumer where T : class
    {
        void Consume(T message);
    }
}