using System;
using RabbitMQ.Client;

namespace ManagedShovel
{
    public class ToDescriptor
    {
        private readonly ManagedShovelConfiguration configuration;

        internal ToDescriptor(ManagedShovelConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public ToDeclarationsDescriptor Declarations(params Action<IModel>[] declarations)
        {
            configuration.DestinationDeclarations = declarations;
            return new ToDeclarationsDescriptor(configuration);
        }
    }
}