using Roger;

namespace Tests.Unit.SupportClasses
{
    public class SomeExchangeRogerMessageAttribute : RogerMessageAttribute
    {
        public SomeExchangeRogerMessageAttribute() : base("SomeExchange")
        {
        }
    }
}