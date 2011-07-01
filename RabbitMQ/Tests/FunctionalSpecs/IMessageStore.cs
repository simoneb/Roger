using System;
using System.Collections.Generic;
using RabbitMQ.Client.Events;

namespace Tests.FunctionalSpecs
{
    public interface IMessageStore : IEnumerable<BasicDeliverEventArgs>, IDisposable
    {
        void Store(BasicDeliverEventArgs message);
    }
}