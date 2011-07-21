using System;

namespace Tests.Integration.Observable
{
    public interface IRabbitObservable : IObservable<RabbitMessage>
    {
        IDisposable Subscribe(IObserver<RabbitMessage> observer, QueueOptions queue = default(QueueOptions));
    }
}