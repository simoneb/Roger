using System;
using System.Threading.Tasks;
using Common;
using MbUnit.Framework;
using Resbit;
using Shoveling.Test.Utils;

namespace Shoveling.Test
{
    [TestFixture]
    public class With_rabbitmq_broker
    {
        protected static RabbitMQBroker Broker { get { return Bootstrap.Broker;  } }
        protected static readonly ResbitClient RestClient = new ResbitClient(Globals.HostName, "guest", "guest");

        [FixtureSetUp]
        public void Check_broker_running()
        {
            Assert.IsNotNull(RestClient.Overview(), "Broker does not appear to be running");
        }

        protected Task<TResult> Start<TResult>(Func<TResult> function)
        {
            return Task<TResult>.Factory.StartNew(function);
        }

        protected Task Start(Action function)
        {
            return Task.Factory.StartNew(function);
        }
    }
}