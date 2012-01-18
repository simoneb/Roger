using System.Collections.Generic;
using System.IO;
using Common;
using MbUnit.Framework;
using RabbitMQ.Client;
using Roger;
using Tests.Integration.Bus.SupportClasses;

namespace Tests.Integration.Bus
{
    public abstract class With_default_bus : With_rabbitmq_broker
    {
        protected DefaultRogerBus Bus;
        private SimpleConsumerContainer consumerContainer;
        protected IModel TestModel;
        private IConnection localConnection;

        [SetUp]
        public void InitializeBus()
        {
            consumerContainer = new SimpleConsumerContainer();

            Bus = new DefaultRogerBus(ConnectionFactory,
                                      consumerContainer,
                                      idGenerator: IdGenerator,
                                      sequenceGenerator: SequenceGenerator,
                                      messageFilters: MessageFilters);

            localConnection = Helpers.CreateConnection();
            TestModel = localConnection.CreateModel();
            TestModel.ExchangeDeclare("TestExchange", 
                                      ExchangeType.Topic, 
                                      true /* to have the exchange there when restarting broker app within tests */, 
                                      false, 
                                      null);

            BeforeBusInitialization();

            Bus.Start();

            AfterBusInitialization();
        }

        protected virtual IConnectionFactory ConnectionFactory
        {
            get { return new ManualConnectionFactory(Helpers.CreateConnection); }
        }

        protected virtual IIdGenerator IdGenerator { get { return Default.IdGenerator;} }

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

            AfterBusDispose();

            try
            {
                localConnection.Dispose();
            }
            catch (IOException)
            {
                // if the broker was restarted in tests this connection would be closed and
                // closing it would throw IOException as the socket is closed
            }
        }

        protected virtual void AfterBusDispose()
        {
            
        }

        protected void RegisterOnDefaultBus(IConsumer consumer)
        {
            consumerContainer.Register(consumer);
        }
    }
}