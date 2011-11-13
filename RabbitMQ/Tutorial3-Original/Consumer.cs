using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Tutorial3_Original
{
    public class Consumer
    {
        public static void Start()
        {
            var connectionFactory = new ConnectionFactory {HostName = "localhost"};

            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare("my_exchange", ExchangeType.Fanout, false, true, null);
                var queue = channel.QueueDeclare("", false, true, true, null);

                channel.QueueBind(queue, "my_exchange", "");

                var consumer = new EventingBasicConsumer {Model = channel};

                consumer.Received += (_, args) => Console.WriteLine(Encoding.UTF8.GetString(args.Body));

                channel.BasicConsume(queue, true, consumer);
            }
        }
    }
}