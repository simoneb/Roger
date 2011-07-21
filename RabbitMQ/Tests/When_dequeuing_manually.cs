using System.Collections.Generic;
using System.IO;
using MbUnit.Framework;

namespace Tests.Integration
{
    public class When_dequeuing_manually : QueueingBasicConsumer_behavior
    {
        protected override IEnumerable<object> Consume
        {
            get
            {
                while (true)
                    yield return Consumer.Queue.Dequeue();
            }
        }

        [Test]
        public void When_model_closes_will_throw_and_queue_will_not_deliver_any_messages()
        {
            Model.Dispose();

            Assert.IsInstanceOfType<EndOfStreamException>(WaitForError);

            Assert.AreSame(InitialMessageValue, WaitForCompletion);
        }

        [Test]
        public void When_connection_closes_will_throw_and_queue_will_not_deliver_any_messages()
        {
            Connection.Dispose();

            Assert.IsInstanceOfType<EndOfStreamException>(WaitForError);

            Assert.AreSame(InitialMessageValue, WaitForCompletion);
        }
    }
}