using System;
using System.Threading;
using System.Threading.Tasks;
using MbUnit.Framework;
using Resbit;
using Spring.Messaging.Amqp.Rabbit.Admin;

namespace Tests.Integration
{
    [TestFixture]
    public class With_rabbitmq_broker
    {
        protected static RabbitBrokerAdmin Broker { get { return Bootstrapper.Broker; } }
        protected static ResbitClient BrokerHttp { get { return Bootstrapper.BrokerHttp; } }

        protected static void StartFederationProxy()
        {
            Bootstrapper.StartFederationProxy();
        }

        protected static void StopFederationProxy()
        {
            Bootstrapper.StopFederationProxy();
        }

        protected static void StartAlternativePortProxy()
        {
            Bootstrapper.StartAlternativePortProxy();
        }

        protected static void StopAlternativePortProxy()
        {
            Bootstrapper.StopAlternativePortProxy();
        }

        protected Tuple<Task<TResult>, CancellationTokenSource> Start<TResult>(Func<TResult> function)
        {
            var tokenSource = new CancellationTokenSource();
            var task = Task<TResult>.Factory.StartNew(function, tokenSource.Token)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted) throw t.Exception;

                    return t.Result;
                }, tokenSource.Token);

            return Tuple.Create(task, tokenSource);
        }

        protected static CancellationTokenSource Start(Action function)
        {
            var tokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(function, tokenSource.Token).ContinueWith(t => { if (t.IsFaulted) throw t.Exception; });
            return tokenSource;
        }
    }
}