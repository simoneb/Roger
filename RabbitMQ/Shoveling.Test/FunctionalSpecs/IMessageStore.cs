using System;
using System.Collections.Generic;
using RabbitMQ.Client.Events;

namespace Shoveling.Test.FunctionalSpecs
{
    public interface IMessageStore : IEnumerable<BasicDeliverEventArgs>, IDisposable
    {
        void Store(BasicDeliverEventArgs message);
        int Count { get; }
    }
}