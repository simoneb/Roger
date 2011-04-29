using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Common;
using RabbitMQ.Client;

namespace Tutorial3
{
    public class Runner : IProcessesProvider
    {
        public IEnumerable<IProcess> Processes { get
        {
            yield return new Publisher();
            yield return new Consumer();
        } }
    }

    public class Consumer : IProcess
    {
        public void Start(WaitHandle waitHandle)
        {
            var connectionFactory = new ConnectionFactory { HostName = Globals.HostName };

            using (var connection = connectionFactory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare("tutorial3_exchange", "fanout");

                    var queue = channel.QueueDeclare(null, false, true, true, null);
                    //channel.QueueBind(queue, "tutori");
                }
            }
        }
    }

    public class Publisher : IProcess
    {
        public void Start(WaitHandle waitHandle)
        {
            var connectionFactory = new ConnectionFactory {HostName = Globals.HostName};

            using (var connection = connectionFactory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(Constants.ExchangeName, "fanout");

                    while (!waitHandle.WaitOne(TimeSpan.FromSeconds(5)))
                    {
                        channel.BasicPublish(Constants.ExchangeName, "", null,
                                             Encoding.UTF8.GetBytes("Ciao " + DateTime.Now.ToLongTimeString()));
                        Console.WriteLine("Message published");
                    }
                }
            }
        }
    }

    public class Constants
    {
        public const string ExchangeName = "tutorial3_exchange";
    }
}
