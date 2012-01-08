using System;
using RabbitMQ.Client;

namespace Roger.Chat.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionFactory = new DefaultConnectionFactory("localhost");

            using(var connection = connectionFactory.CreateConnection())
            {
                var model = connection.CreateModel();

                model.ExchangeDeclare("RabbusChat", ExchangeType.Topic, false);
            }

            var consumerContainer = new SimpleConsumerContainer();

            var bus = new DefaultRogerBus(connectionFactory, consumerContainer, exchangeResolver: new StaticExchangeResolver("RabbusChat"));
            var chat = new ChatServer(bus);

            consumerContainer.Register(chat);

            bus.Start();

            Console.ReadLine();

            bus.Dispose();
        }
    }
}