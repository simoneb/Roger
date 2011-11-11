using System;
using System.IO;
using System.Threading;
using Common;
using MbUnit.Framework;
using RabbitMQ.Client;
using Rabbus;
using Rabbus.Resolvers;
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
                                       log: new DebugLog());

            localConnection = Helpers.CreateConnection();
            TestModel = localConnection.CreateModel();
            TestModel.ExchangeDeclare("TestExchange", 
                                      ExchangeType.Direct, 
                                      true /* to have the exchange there when restarting broker app within tests */, 
                                      false, 
                                      null);

            BeforeBusInitialization();

            Bus.Initialize();

            AfterBusInitialization();
        }

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
                // if the broker was restarted in tests this connections would be closed and
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