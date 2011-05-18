using MbUnit.Framework;

namespace Shoveling.Test
{
    [AssemblyFixture]
    public sealed class With_rabbitmq_server
    {
        [FixtureSetUp]
        public void TestFixtureSetup()
        {
            Broker.Instance = new RabbitMQBroker(@"..\..\..\..\RabbitMQServer");
            Broker.Instance.Start();
        }

        [FixtureTearDown]
        public void TestFixtureTeardown()
        {
            Broker.Instance.Stop();
        }
    }
}