using System;
using Roger;

namespace Tests.Unit.SupportClasses
{
    public class MyThrowingConsumer : IConsumer<MyMessage>
    {
        public void Consume(MyMessage message)
        {
            throw new InvalidOperationException("sorry :(");
        }
    }
}