using System;
using System.IO;
using System.Threading;
using Common;
using MbUnit.Framework;
using RabbitMQ.Client;
using Rabbus;
using Rabbus.Reflection;
using Rabbus.Resolvers;
using Rabbus.Serialization;
using Tests.Integration.Bus.SupportClasses;

namespace Tests.Integration.Bus
{
    public abstract class With_default_bus : With_rabbitmq_broker
    {
        protected DefaultRabbitBus Bus;
        private IConnection connection;
        protected ManualRegistrationConsumerResolver ConsumerResolver;
        private DefaultRoutingKeyResolver routingKeyResolver;
        private DefaultTypeResolver typeResolver;
        private ProtoBufNetSerializer serializer;
        protected IModel TestModel;
        private IConnection localConnection;

        [SetUp]
        public void InitializeBus()
        {
            routingKeyResolver = new DefaultRoutingKeyResolver();
            typeResolver = new DefaultTypeResolver();
            serializer = new ProtoBufNetSerializer();

            var consumerToMessageTypes = new DefaultSupportedMessageTypesResolver();
            ConsumerResolver = new ManualRegistrationConsumerResolver(consumerToMessageTypes);
            Bus = new DefaultRabbitBus(new IdentityConnectionFactory(Helpers.CreateConnection),
                                       ConsumerResolver,
                                       typeResolver, 
                                       consumerToMessageTypes, 
                                       new DefaultExchangeResolver(), 
                                       routingKeyResolver, 
                                       serializer, 
                                       new DefaultReflection(), 
                                       new DebugLog());

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

        protected virtual void AfterBusInitialization()
        {
        }

        protected virtual void BeforeBusInitialization()
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
            catch (IOException e)
            {
                // happens sometimes, figure out why
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
    }
}