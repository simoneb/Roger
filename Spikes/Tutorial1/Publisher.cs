using System;
using System.Text;
using System.Threading;
using Common;
using RabbitMQ.Client;

namespace Tutorial1
{
    public class Publisher : IProcess
    {
        public void Start(WaitHandle waitHandle)
        {
            var factory = new ConnectionFactory { HostName = Globals.HostName };

            using (var connection = factory.CreateConnection())
            using (var model = connection.CreateModel())
            {
                model.QueueDeclare(Constants.QueueName, false, false, false, null);

                model.BasicPublish("", Constants.QueueName, null, Encoding.UTF8.GetBytes("ciao!"));

                Console.WriteLine("Published!");
            }
        }
    }
}
