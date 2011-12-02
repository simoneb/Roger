namespace Tests.Unit.SupportClasses
{
    public abstract class MyBaseMessage {}

    class MyOtherDerivedMessage : MyBaseMessage {}

    class MyDerivedMessage : MyBaseMessage {}

    abstract class BaseMessageWithAbstractDerived {}

    abstract class AbstractDerived : BaseMessageWithAbstractDerived {}

    class NonAbstractDerived : BaseMessageWithAbstractDerived {}
}