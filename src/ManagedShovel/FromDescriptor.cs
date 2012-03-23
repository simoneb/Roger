using System;
using RabbitMQ.Client;

namespace ManagedShovel
{
    public class FromDescriptor
    {
        private readonly ManagedShovelConfiguration configuration;

        internal FromDescriptor(ManagedShovelConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public SourceDeclarationsDescriptor Declarations(params Action<IModel>[] declarations)
        {
            configuration.SourceDeclarations = declarations;

            return new SourceDeclarationsDescriptor(configuration);
        }
    }
}