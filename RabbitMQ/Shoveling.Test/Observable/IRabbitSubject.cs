using System;
using System.Collections.Generic;

namespace Shoveling.Test.Bus
{
    public interface IRabbitObserver<T> : IObserver<RabbitMessage<T>>, IRabbitObserver
    {
    }

    public interface IRabbitObserver : IObserver<RabbitMessage>
    {
        void AddBindingsTo(ICollection<string> collector);        
    }
}