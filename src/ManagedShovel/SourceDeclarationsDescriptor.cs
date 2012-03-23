using System.Linq;

namespace ManagedShovel
{
    public class SourceDeclarationsDescriptor
    {
        private readonly ManagedShovelConfiguration configuration;

        internal SourceDeclarationsDescriptor(ManagedShovelConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public ToDescriptor To(string broker, params string[] brokers)
        {
            configuration.Destinations = new[] {broker}.Concat(brokers).ToArray();
            return new ToDescriptor(configuration);
        }
    }
}