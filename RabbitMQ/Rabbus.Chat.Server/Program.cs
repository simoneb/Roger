using System;
using RabbitMQ.Client;
using Rabbus.Resolvers;

namespace Rabbus.Chat.Server
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

            var consumerResolver = new ManualRegistrationConsumerResolver(new DefaultSupportedMessageTypesResolver());

            var bus = new DefaultRabbitBus(connectionFactory, consumerResolver, exchangeResolver: new StaticExchangeResolver("RabbusChat"));
            var chat = new ChatServer(bus);

            consumerResolver.Register(chat);

            bus.Start();

            Console.ReadLine();

            bus.Dispose();
        }
    }
}