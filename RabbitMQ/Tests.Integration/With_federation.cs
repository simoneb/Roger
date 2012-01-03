using System.Threading;
using MbUnit.Framework;
using Tests.Integration.Utils;

namespace Tests.Integration
{
    public class With_federation : With_rabbitmq_broker
    {
        [FixtureSetUp]
        public void Setup()
        {
            // leave some time to TcpTrace to reestabilish the connection to the server
            Thread.Sleep(2000);
        }

        protected static void StartFederationLink()
        {
            Bootstrap.StartFederationLink();
        }

        protected static void ShutdownFederationLink()
        {
            Bootstrap.StopFederationLink();
        }
    }
}