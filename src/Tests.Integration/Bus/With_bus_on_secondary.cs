using Common;
using Roger;
using Tests.Integration.Bus.SupportClasses;

namespace Tests.Integration.Bus
{
    public abstract class With_bus_on_secondary : With_default_bus
    {
        protected RogerBus SecondaryBus;
        private SimpleConsumerContainer secondaryConsumerContainer;

        protected override void AfterBusInitialization()
        {
            secondaryConsumerContainer = new SimpleConsumerContainer();

            BeforeSecondaryBusInitialization();

            SecondaryBus = new RogerBus(new ManualConnectionFactory(Helpers.CreateSecondaryConnectionToMainVirtualHost),
                                        secondaryConsumerContainer, options: new RogerOptions(prefetchCount: null));

            SecondaryBus.Start();

            AfterSecondaryBusInitialization();
        }

        protected void RegisterOnSecondaryBus(IConsumer consumer)
        {
            secondaryConsumerContainer.Register(consumer);
        }

        protected virtual void AfterSecondaryBusInitialization()
        {
            
        }

        protected virtual void BeforeSecondaryBusInitialization()
        {
            
        }

        protected override void AfterBusDispose()
        {
            SecondaryBus.Dispose();
        }
    }
}