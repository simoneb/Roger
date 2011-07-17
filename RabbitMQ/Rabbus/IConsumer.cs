namespace Rabbus
{
    public interface IConsumer<in T> : IRabbusConsumer
    {
        void Consume(T message);
    }
}