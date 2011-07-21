using System;
using System.ServiceModel;
using Common;
using MbUnit.Framework;
using RabbitMQ.ServiceModel;

namespace Tests.Integration.WCF
{
    public class Wcf_bindings : With_rabbitmq_broker
    {
        [Test]
        public void OneWay()
        {
            var host = new ServiceHost(typeof(Logger), new Uri("soap.amqp:///"));

            var binding = new RabbitMQBinding(Globals.HostName, Globals.Port){OneWayOnly = true};

            host.AddServiceEndpoint(typeof(ILogger), binding, "Log");

            host.Open();

            var client = ChannelFactory<ILogger>.CreateChannel(binding, new EndpointAddress("soap.amqp:///Log"));

            client.Log("ciao");

            Logger.Semaphore.WaitOne(500);

            Assert.AreEqual("ciao", Logger.LastLogged);

            host.Close();
        }

        [Test]
        public void TwoWay()
        {
            var host = new ServiceHost(typeof(Calculator), new Uri("soap.amqp:///"));

            var binding = new RabbitMQBinding(Globals.HostName, Globals.Port);

            host.AddServiceEndpoint(typeof(ICalculator), binding, "Calculator");

            host.Open();

            var client = ChannelFactory<ICalculator>.CreateChannel(binding, new EndpointAddress("soap.amqp:///Calculator"));

            Assert.AreEqual(3, client.Add(1, 2));
                
            host.Close();
        }

        [Test]
        public void Duplex()
        {
            var host = new ServiceHost(typeof(OrderService), new Uri("soap.amqp:///"));

            var binding = new RabbitMQBinding(Globals.HostName, Globals.Port);

            host.AddServiceEndpoint(typeof(IOrderService), binding, "OrderService");

            host.Open();

            var callback = new OrderCallback();
            var client = DuplexChannelFactory<IOrderService>.CreateChannel(callback,
                                                                           binding,
                                                                           new EndpointAddress("soap.amqp:///OrderService"));

            client.Order();

            callback.Semaphore.WaitOne(500);
            Assert.IsTrue(callback.Completed);

            host.Close();
        }
    }
}