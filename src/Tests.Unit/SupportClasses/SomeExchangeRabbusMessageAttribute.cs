using Rabbus;

namespace Tests.Unit.SupportClasses
{
    public class SomeExchangeRabbusMessageAttribute : RabbusMessageAttribute
    {
        public SomeExchangeRabbusMessageAttribute() : base("SomeExchange")
        {
        }
    }
}