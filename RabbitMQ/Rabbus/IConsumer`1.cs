namespace Rabbus
{
    public interface IConsumer<in T> : IConsumer
    {
        void Consume(T message);
    }
}