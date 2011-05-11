using System;
using MbUnit.Framework;

namespace Resbit.Test
{
    [TestFixture]
    public class Exchanges : ResbitTest
    {
        [Test]
        public void Not_null()
        {
            Assert.IsNotNull(Client.Exchanges());
        }

        [Test]
        public void Not_empty()
        {
            Assert.GreaterThan(Client.Exchanges().Length, 0);
        }

        [Test]
        public void First_exchange_name()
        {
            Assert.IsNotNull(Client.Exchanges()[0].name);
        }

        [TestFixture]
        public class Vhost : ResbitTest
        {
            [Test]
            public void Not_null()
            {
                Assert.IsNotNull(Client.Exchanges("/"));
            }

            [Test]
            public void Not_empty()
            {
                Assert.GreaterThan(Client.Exchanges("/").Length, 0);
            }

            [Test]
            public void First_exchange_name()
            {
                Assert.IsNotNull(Client.Exchanges("/")[0].name);
            }

            [TestFixture]
            public class Exchange : ResbitTest
            {
                [Test]
                public void Not_null()
                {
                    Assert.IsNotNull(Client.GetExchange(Client.Exchanges()[0].name));
                }

                [Test]
                public void First_exchange_name()
                {
                    Assert.IsNotNull(Client.GetExchange(Client.Exchanges()[1].name).name);
                }

                [Test]
                public void Put()
                {
                    Client.PutExchange(Guid.NewGuid().ToString());
                }
            }
        }
    }
}