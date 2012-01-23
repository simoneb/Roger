using System.Collections.Generic;
using System.IO;
using Common;
using MbUnit.Framework;
using RabbitMQ.Client;
using Roger;
using Roger.Internal.Impl;
using Tests.Integration.Bus.SupportClasses;
using System.Linq;

namespace Tests.Integration.Bus
{
    public abstract class With_default_bus : With_rabbitmq_broker
    {
        protected RogerBus Bus;
        private SimpleConsumerContainer consumerContainer;
        protected IModel TestModel;
        private IConnection localConnection;

        [SetUp]
        public void InitializeBus()
        {
            consumerContainer = new SimpleConsumerContainer();

            Bus = new RogerBus(ConnectionFactory,
                               consumerContainer,
                               idGenerator: IdGenerator,
                               sequenceGenerator: SequenceGenerator,
                               options: new RogerOptions(prefetchCount: null /*no safety nets during tests*/));

            Bus.Filters.Add(MessageFilters.ToArray());

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

        protected virtual IIdGenerator IdGenerator { get { return new RandomIdGenerator();} }

        protected virtual IEnumerable<IMessageFilter> MessageFilters { get { yield break; } }
        protected virtual ISequenceGenerator SequenceGenerator { get { return new ByMessageHirarchyRootSequenceGenerator(); } }

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