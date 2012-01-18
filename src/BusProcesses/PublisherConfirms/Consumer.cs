using System;
using System.Threading;
using Common;
using Roger;

namespace BusProcesses.PublisherConfirms
{
    [Serializable]
    public class Consumer : IProcess
    {
        public void Start(WaitHandle waitHandle)
        {
            var connectionFactory = new DefaultConnectionFactory(Globals.MainHostName);

            PublisherConfirmsProvider.DeclareExchange(connectionFactory);
            var bus = new RogerBus(connectionFactory, new SimpleConsumerContainer(new PublisherConfirmsConsumer()));
            bus.Start();

            waitHandle.WaitOne();

            bus.Dispose();
        }
    }
}