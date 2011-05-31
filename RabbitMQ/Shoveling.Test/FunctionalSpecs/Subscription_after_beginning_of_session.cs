using MbUnit.Framework;

namespace Shoveling.Test.FunctionalSpecs
{
    [TestFixture]
    public class Subscription_after_beginning_of_session : With_rabbitmq_broker
    {
        [Test]
        public void TEST_NAME()
        {
            Start(Producer);
        }
    }
}