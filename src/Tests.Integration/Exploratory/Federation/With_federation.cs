using System.Threading;
using MbUnit.Framework;

namespace Tests.Integration.Exploratory.Federation
{
    public class With_federation : With_rabbitmq_broker
    {
        [FixtureSetUp]
        public void FixtureSetup()
        {
            // leave some time to TcpTrace to reestabilish the connection to the server
            Thread.Sleep(2000);
        }
    }
}