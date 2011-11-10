namespace Rabbus.GuidGeneration
{
    internal class RandomGuidGenerator : IGuidGenerator
    {
        public RabbusGuid Next()
        {
            return RabbusGuid.NewGuid();
        }
    }
}