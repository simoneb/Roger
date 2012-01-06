namespace Roger.Internal.Impl
{
    internal class RandomIdGenerator : IIdGenerator
    {
        public RogerGuid Next()
        {
            return RogerGuid.NewGuid();
        }
    }
}