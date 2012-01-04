using System;
using System.Text;
using RabbitMQ.Client;

namespace Tutorial3_Original
{
    public class Publisher
    {
        public static void Start()
        {
            var connectionFactory = new ConnectionFactory {HostName = "localhost"};

            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare("my_exchange", ExchangeType.Fanout, false, true, null);
                channel.BasicPublish("my_exchange", "", null, Encoding.UTF8.GetBytes("Ciao"));

                Console.WriteLine("Message published");
            }
        }
    }
}