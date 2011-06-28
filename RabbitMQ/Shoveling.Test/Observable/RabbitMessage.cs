using System;
using RabbitMQ.Client.Events;

namespace Shoveling.Test.Bus
{
    public class RabbitMessage 
    {
        private readonly BasicDeliverEventArgs m_message;

        public RabbitMessage(BasicDeliverEventArgs message)
        {
            m_message = message;
        }

        public Type Type { get { return Type.GetType(m_message.BasicProperties.Type); } }
    }

    public class RabbitMessage<T> 
    {
        private readonly RabbitMessage m_raw;

        public RabbitMessage(RabbitMessage raw)
        {
            m_raw = raw;
        }

        public T Body { get; set; }

        public Type Type { get { return m_raw.Type; } }
    }
}