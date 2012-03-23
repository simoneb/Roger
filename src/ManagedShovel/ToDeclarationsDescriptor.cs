namespace ManagedShovel
{
    public class ToDeclarationsDescriptor
    {
        private readonly ManagedShovelConfiguration configuration;

        internal ToDeclarationsDescriptor(ManagedShovelConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public OptionsDescriptor UseLastCreatedQueue()
        {
            configuration.LastCreatedQueue = true;
            return new OptionsDescriptor(configuration);
        }

        public OptionsDescriptor UseQueue(string queueName)
        {
            configuration.Queue = queueName;
            return new OptionsDescriptor(configuration);
        }
    }
}