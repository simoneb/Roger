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
            Client = new ResbitClient(Globals.HostName, "guest", "guest");
        }
    }
}