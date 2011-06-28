using System;
using MbUnit.Framework;
using Shoveling.Test.Bus;

namespace Shoveling.Test.Observable
{
    public class ObservableTests
    {
        [Test]
        public void TEST_NAME()
        {
            var observable = new RabbitObservable("testExchange");

            observable.Subscribe(new MySubObserver());
            observable.Subscribe(new MyObserver());
        }
    }

    public class MyObserver : RabbitObserver<MyMessage>.And<MySubMessage>
    {
        public override void OnNext(RabbitMessage<MyMessage> value)
        {
        }

        public override void OnNext(RabbitMessage<MySubMessage> value)
        {
        }

        public override void OnError(Exception error)
        {
        }

        public override void OnCompleted()
        {
        }
    }

    public class MySubObserver : RabbitObserver<MySubMessage>
    {
        public override void OnNext(RabbitMessage<MySubMessage> value)
        {
        }

        public override void OnError(Exception error)
        {
        }

        public override void OnCompleted()
        {
        }
    }

    public class MySubMessage : MyMessage
    {
    }

    public class MyMessage
    {
    }
}