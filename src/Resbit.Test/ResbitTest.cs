using Common;
using MbUnit.Framework;

namespace Resbit.Test
{
    public class ResbitTest
    {
        protected ResbitClient Client;

        [FixtureSetUp]
        public void SetupBase()
        {
            Client = new ResbitClient(Constants.HostName, "guest", "guest");
        }
    }
}