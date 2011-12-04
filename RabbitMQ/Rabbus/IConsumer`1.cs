namespace Rabbus
{
    public interface IConsumer<T> : IConsumer where T : class
    {
        void Consume(T message);
    }
}