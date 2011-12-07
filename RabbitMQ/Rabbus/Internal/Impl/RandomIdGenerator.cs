namespace Rabbus.Internal.Impl
{
    internal class RandomIdGenerator : IIdGenerator
    {
        public RabbusGuid Next()
        {
            return RabbusGuid.NewGuid();
        }
    }
}