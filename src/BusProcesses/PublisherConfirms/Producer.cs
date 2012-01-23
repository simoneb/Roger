using System;
using System.Threading;
using Common;
using Roger;

namespace BusProcesses.PublisherConfirms
{
    [Serializable]
    public class Producer : IProcess
    {
        public void Start(WaitHandle waitHandle)
        {
            var connectionFactory = new DefaultConnectionFactory(Constants.HostName);

            PublisherConfirmsProvider.DeclareExchange(connectionFactory);

            var bus = new RogerBus(connectionFactory);
            bus.Start();
            StartPublishing(bus, waitHandle);
        }

        private void StartPublishing(IRabbitBus bus, WaitHandle waitHandle)
        {
            var counter = 0;

            while (!waitHandle.WaitOne(10))
            {
                bus.Publish(new PublisherConfirmsMessage {Counter = ++counter}, persistent: false);
                Console.WriteLine("Published message {0}", counter);
            }

            bus.Dispose();
        }
    }
}