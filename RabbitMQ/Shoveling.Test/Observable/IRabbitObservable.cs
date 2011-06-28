using System;

namespace Shoveling.Test.Bus
{
    public interface IRabbitObservable : IObservable<RabbitMessage>
    {
        IDisposable Subscribe(IObserver<RabbitMessage> observer, QueueOptions queue = default(QueueOptions));
    }
}