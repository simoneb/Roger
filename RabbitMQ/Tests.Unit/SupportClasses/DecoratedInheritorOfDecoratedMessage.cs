using Rabbus;

namespace Tests.Unit.SupportClasses
{
    [RabbusMessage("whatever")]
    public class DecoratedInheritorOfDecoratedMessage : DecoratedMessageBase
    {
    }
}