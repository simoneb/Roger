namespace Rabbus
{
    public interface IConsumer<in T> : IConsumer where T : class
    {
        void Consume(T message);
    }

    public abstract class Consumer<T> : IConsumer<T> where T : class
    {
        public abstract void Consume(T message);

        public interface AndDerivedInSameAssembly : IConsumer<T>
        {
        }
    }
}