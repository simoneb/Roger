using Rabbus;

namespace Tests.Unit.SupportClasses
{
    public class RabbusMessageInheritorAttribute : RabbusMessageAttribute
    {
        public RabbusMessageInheritorAttribute(string exchange) : base(exchange)
        {
            
        }
    }
}