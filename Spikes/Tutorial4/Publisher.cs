using System;
using System.Text;
using System.Threading;
using Common;
using RabbitMQ.Client;

namespace Tutorial4
{
    [Serializable]
    public class Publisher : IProcess
    {
        public void Start(WaitHandle waitHandle)
        {
            var connection = new ConnectionFactory {HostName = Globals.HostName}.CreateConnection();
            var model = connection.CreateModel();

            model.ExchangeDeclare(Constants.ExchangeName, ExchangeType.Direct);

            while(!waitHandle.WaitOne(TimeSpan.FromSeconds(5)))
            {
                var severity = Severities.Random();
                model.BasicPublish(Constants.ExchangeName, severity, null, GetBody(severity));
                Console.WriteLine("Published {0} message", severity);
            }
        }

        private static byte[] GetBody(string routingKey)
        {
            return Encoding.UTF8.GetBytes(routingKey + " " + DateTime.Now.ToLongTimeString());
        }

    }
}