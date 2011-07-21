namespace Rabbus
{
    public interface IConsumer<in T> : IConsumer where T :  new()
    {
        void Consume(T message);
    }
}