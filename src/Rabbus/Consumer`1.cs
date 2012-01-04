namespace Rabbus
{
    public abstract class Consumer<T> where T : class
    {
        public interface SubclassesInSameAssembly : IConsumer
        {
            void Consume(T message);
        }
    }
}