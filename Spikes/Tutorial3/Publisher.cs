using System;
using System.Text;
using System.Threading;
using Common;
using RabbitMQ.Client;

namespace Tutorial3
{
    [Serializable]
    public class Publisher : IProcess
    {
        public void Start(WaitHandle waitHandle)
        {
            var connectionFactory = new ConnectionFactory {HostName = Globals.HostName};

            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(Constants.ExchangeName, ExchangeType.Fanout);

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