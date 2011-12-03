namespace Rabbus
{
    public interface IConsumer<T> : IConsumer where T : class
    {
        void Consume(T message);
    }

    public abstract class Consumer<T> where T : class
    {
        public interface SubclassesInSameAssembly : IConsumer
        {
            void Consume(T message);
        }
    }
}