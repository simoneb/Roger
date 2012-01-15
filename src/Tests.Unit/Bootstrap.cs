using Common;
using MbUnit.Framework;

namespace Tests.Unit
{
    [AssemblyFixture]
    public class Bootstrap
    {
         [FixtureSetUp]
         public void Setup()
         {
             Helpers.InitializeTestLogging();
         }
    }
}