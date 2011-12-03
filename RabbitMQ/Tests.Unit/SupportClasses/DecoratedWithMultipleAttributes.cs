using Rabbus;

namespace Tests.Unit.SupportClasses
{
    [RabbusMessage("a")]
    [RabbusMessageInheritor("b")]
    public class DecoratedWithMultipleAttributes
    {
    }
}