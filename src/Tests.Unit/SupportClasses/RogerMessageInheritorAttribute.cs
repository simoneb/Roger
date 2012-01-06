using Roger;

namespace Tests.Unit.SupportClasses
{
    public class RogerMessageInheritorAttribute : RogerMessageAttribute
    {
        public RogerMessageInheritorAttribute(string exchange) : base(exchange)
        {
            
        }
    }
}