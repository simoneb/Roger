using System;
using Roger;

namespace BusProcesses.PublisherConfirms
{
    public class PublisherConfirmsConsumer : IConsumer<PublisherConfirmsMessage>
    {
        public void Consume(PublisherConfirmsMessage message)
        {
            Console.WriteLine("Received message {0}", message.Counter);
        }
    }
}