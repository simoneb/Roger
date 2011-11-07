using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Common;
using MbUnit.Framework;
using Resbit;
using Tests.Integration.Utils;

namespace Tests.Integration
{
    [TestFixture]
    public class With_rabbitmq_broker
    {
        protected static RabbitMQBroker Broker { get { return Bootstrap.Broker;  } }
        protected static readonly ResbitClient RestClient = new ResbitClient(Globals.HostName, "guest", "guest");

        [FixtureSetUp]
        public void Check_broker_running()
        {
            try
            {
                Assert.IsNotNull(RestClient.Overview(), "Broker does not appear to be running");
            }
            catch (WebException e)
            {
                Assert.Fail("Broker does not appear to be running: {0}", e);
            }
        }

        protected Tuple<Task<TResult>, CancellationTokenSource> Start<TResult>(Func<TResult> function)
        {
            var tokenSource = new CancellationTokenSource();
            var task = Task<TResult>.Factory.StartNew(function, tokenSource.Token)
                .ContinueWith(t =>
                {
                    if(t.IsFaulted) throw t.Exception;

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