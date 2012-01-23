namespace Roger
{
    /// <summary>
    /// Marker interface
    /// </summary>
    public interface IConsumer
    {
    }

    public interface IConsumer<T> : IConsumer where T : class
    {
        void Consume(T message);
    }

    public interface IConsumer1<T> : IConsumer where T : class
    {
        void Consume(T message);
    }

    public interface IConsumer2<T> : IConsumer where T : class
    {
        void Consume(T message);
    }
}