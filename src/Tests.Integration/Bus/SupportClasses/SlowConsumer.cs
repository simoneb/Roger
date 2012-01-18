using System;
using System.Threading;

namespace Tests.Integration.Bus.SupportClasses
{
    public class SlowConsumer<T> : GenericMultipleArrivalsConsumer<MyMessage>
    {
        private readonly TimeSpan delay;

        public SlowConsumer(TimeSpan delay, int expectedDeliveries) : base(expectedDeliveries)
        {
            this.delay = delay;
        }

        public override void Consume(MyMessage message)
        {
            base.Consume(message);
            Thread.Sleep(delay);
        }
    }
}