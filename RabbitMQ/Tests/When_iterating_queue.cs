using System.Collections.Generic;
using System.Linq;
using MbUnit.Framework;

namespace Tests.Integration
{
    public class When_iterating_queue : QueueingBasicConsumer_behavior
    {
        protected override IEnumerable<object> Consume { get { return Consumer.Queue.OfType<object>(); } }

        [Test]
        public void When_model_closes_will_not_throw_and_queue_will_not_deliver_any_messages()
        {
            Model.Dispose();

            Assert.AreSame(InitialMessageValue, WaitForCompletion);
        }

        [Test]
        public void When_connection_closes_will_not_throw_and_queue_will_not_deliver_any_messages()
        {
            Connection.Dispose();

            Assert.AreSame(InitialMessageValue, WaitForCompletion);
        }
    }
}