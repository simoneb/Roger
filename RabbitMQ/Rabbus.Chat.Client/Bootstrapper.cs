using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Caliburn.Micro;
using RabbitMQ.Client;
using Rabbus.Chat.Messages;
using Rabbus.Resolvers;

namespace Rabbus.Chat.Client
{
    public class Bootstrapper : Bootstrapper<ShellViewModel>
    {
        private CompositionContainer container;
        private DefaultConnectionFactory connectionFactory;

        protected override void Configure()
        {
            container = new CompositionContainer(
                new AggregateCatalog(AssemblySource.Instance.Select(x => new AssemblyCatalog(x))));

            var consumerResolver = new ManualRegistrationConsumerResolver(new DefaultSupportedMessageTypesResolver());

            connectionFactory = new DefaultConnectionFactory("localhost");
            var bus = new DefaultRabbitBus(connectionFactory, consumerResolver, exchangeResolver: new StaticExchangeResolver("RabbusChat"));

            var batch = new CompositionBatch();

            batch.AddExportedValue<IWindowManager>(new WindowManager());
            batch.AddExportedValue<IEventAggregator>(new EventAggregator());
            batch.AddExportedValue(container);
            batch.AddExportedValue<IRabbitBus>(bus);

            container.Compose(batch);

            consumerResolver.Register(container.GetExportedValue<ShellViewModel>());
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            using (var connection = connectionFactory.CreateConnection())
            {
                var model = connection.CreateModel();

                model.ExchangeDeclare("RabbusChat", ExchangeType.Topic, false);
            }

            var bus = container.GetExportedValue<IRabbitBus>();
            bus.Start();

            base.OnStartup(sender, e);
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            base.OnExit(sender, e);

            var bus = container.GetExportedValue<IRabbitBus>();
            bus.Publish(new ClientDisconnected { Endpoint = bus.LocalEndpoint });

            bus.Dispose();
        }

        protected override void OnUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            base.OnUnhandledException(sender, e);

            OnExit(this, EventArgs.Empty);
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            string contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
            var exports = container.GetExportedValues<object>(contract);

            if (exports.Any())
                return exports.First();

            throw new Exception(string.Format("Could not locate any instances of contract {0}.", contract));
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
        }

        protected override void BuildUp(object instance)
        {
            container.SatisfyImportsOnce(instance);
        }
    }
}