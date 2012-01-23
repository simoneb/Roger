using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Windsor;

namespace Roger.Windsor
{
    public class WindsorConsumerContainer : IConsumerContainer
    {
        private readonly IWindsorContainer container;

        private readonly IEnumerable<Type> consumerInterfaces = new[]
        {
            typeof (IConsumer<>),
            typeof (IConsumer1<>),
            typeof (IConsumer2<>)
        };

        public WindsorConsumerContainer(IWindsorContainer container)
        {
            this.container = container;
        }

        public IEnumerable<IConsumer> Resolve(Type messageRoot)
        {
            return consumerInterfaces.Select(i => i.MakeGenericType(messageRoot)).SelectMany(i => container.ResolveAll(i).Cast<IConsumer>());
        }

        public void Release(IEnumerable<IConsumer> consumers)
        {
            foreach (var consumer in consumers)
                container.Release(consumer);
        }

        public IEnumerable<Type> GetAllConsumerTypes()
        {
            return consumerInterfaces.SelectMany(i => container.Kernel.GetHandlers(i)).Select(h => h.ComponentModel.Implementation);
        }
    }
}
