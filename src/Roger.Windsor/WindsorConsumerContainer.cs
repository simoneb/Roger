using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Windsor;

namespace Roger.Windsor
{
    public class WindsorConsumerContainer : IConsumerContainer
    {
        private readonly IWindsorContainer container;

        public WindsorConsumerContainer(IWindsorContainer container)
        {
            this.container = container;
        }

        public IEnumerable<IConsumer> Resolve(Type consumerType)
        {
            return container.ResolveAll(consumerType).Cast<IConsumer>();
        }

        public void Release(IEnumerable<IConsumer> consumers)
        {
            foreach (var consumer in consumers)
                container.Release(consumer);
        }

        public IEnumerable<Type> GetAllConsumerTypes()
        {
            return container.Kernel.GetHandlers(typeof (IConsumer<>)).Select(handler => handler.ComponentModel.Implementation);
        }
    }
}
