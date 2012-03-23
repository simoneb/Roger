using System;
using RabbitMQ.Client;

namespace ManagedShovel
{
    public class OptionsDescriptor
    {
        private readonly ManagedShovelConfiguration configuration;

        internal OptionsDescriptor(ManagedShovelConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public OptionsDescriptor PrefetchCount(ushort count)
        {
            configuration.PrefetchCount = count;
            return this;
        }

        public OptionsDescriptor AckMode(AckMode ackMode)
        {
            configuration.AckMode = ackMode;
            return this;
        }

        public OptionsDescriptor PublishProperties(params Action<IBasicProperties>[] properties)
        {
            configuration.PublishProperties = properties;
            return this;
        }

        public OptionsDescriptor PublishFields(string exchangeName = null, string routingKey = null)
        {
            configuration.PublishFields = Tuple.Create(exchangeName, routingKey);
            return this;
        }

        public OptionsDescriptor ReconnectDelay(TimeSpan reconnectDelay)
        {
            configuration.ReconnectDelay = reconnectDelay;
            return this;
        }

        public OptionsDescriptor MaxHops(int hops = 1)
        {
            configuration.MaxHops = hops;
            return this;
        }

        public ManagedShovel Start()
        {
            var shovel = new ManagedShovel(configuration);
            shovel.Start();
            return shovel;
        }
    }
}