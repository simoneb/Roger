using System.Collections.Generic;
using System.IO;
using System.Threading;
using Common;
using MbUnit.Framework;
using RabbitMQ.Client;
using Rabbus;
using Rabbus.GuidGeneration;
using Rabbus.Resolvers;
using Rabbus.Sequencing;
using Rabbus.Utilities;
using Tests.Integration.Bus.SupportClasses;

namespace Tests.Integration.Bus
{
    public abstract class With_default_bus : With_rabbitmq_broker
    {
        protected DefaultRabbitBus Bus;
        private ManualRegistrationConsumerResolver consumerResolver;
        protected IModel TestModel;
        private IConnection localConnection;

        [SetUp]
        public void InitializeBus()
        {
            consumerResolver = new ManualRegistrationConsumerResolver(new DefaultSupportedMessageTypesResolver());
            Bus = new DefaultRabbitBus(new IdentityConnectionFactory(Helpers.CreateConnection),
                                       consumerResolver,
                                       log: new DebugLog(),
                                       guidGenerator: GuidGenerator,
                                       messageFilters: MessageFilters,
                                       sequenceGenerator: SequenceGenerator);

            localConnection = Helpers.CreateConnection();
            TestModel = localConnection.CreateModel();
            TestModel.ExchangeDeclare("TestExchange", 
                                      ExchangeType.Direct, 
                                      true /* to have the exchange there when restarting broker app within tests */, 
                                      false, 
                                      null);

            BeforeBusInitialization();

            Bus.Start();

            AfterBusInitialization();
        }

        protected virtual IGuidGenerator GuidGenerator { get { return Default.GuidGenerator;} }

        protected virtual IEnumerable<IMessageFilter> MessageFilters { get { yield break; } }
        protected virtual ISequenceGenerator SequenceGenerator { get { return Default.SequenceGenerator; } }

        protected virtual void BeforeBusInitialization()
        {
        }

        protected virtual void AfterBusInitialization()
        {
        }

        [TearDown]
        public void CloseConnection()
        {
            Bus.Dispose();

            try
            {
                localConnection.Dispose();
            }
            catch (IOException exception)
            {
                // if the broker was restarted in tests this connection would be closed and
                // closing it would throw IOException as the socket is closed
            }
        }

        protected static void WaitForRoundtrip()
        {
            WaitForDelivery();
            WaitForDelivery();
        }

        protected static void WaitForDelivery()
        {
            Thread.Sleep(500);
        }

        protected void Register(IConsumer consumer)
        {
            consumerResolver.Register(consumer);
        }
    }
}