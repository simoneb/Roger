using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Common;
using RabbitMQ.Client;

namespace Tutorial5
{
    [Serializable]
    public class Publisher : IProcess
    {
        private readonly char[] keyParts = new[]{'1', '2', '3', '4', '5'};
        private readonly Random rnd = new Random();

        public void Start(WaitHandle waitHandle)
        {
            using (var connection = Helpers.CreateConnection())
            using (var model = connection.CreateModel())
            {
                model.ExchangeDeclare(Constants.ExchangeName, ExchangeType.Topic, false, true, null);

                while (!waitHandle.WaitOne(TimeSpan.FromSeconds(5)))
                {
                    var routingKey = CreateRoutingKey();
                    model.BasicPublish(Constants.ExchangeName, routingKey, null, GetBody(routingKey));
                    Console.WriteLine("Published {0} message", routingKey);
                }
            }
        }

        private string CreateRoutingKey()
        {
            var stack = new Stack<char>();

            while(stack.Count(c => c.Equals('.')) < 3)
            {
                stack.Push(RandomRoutingKeyPart());
                stack.Push('.');
            }

            return new string(stack.ToArray()).Trim('.');
        }

        private char RandomRoutingKeyPart()
        {
            return keyParts[rnd.Next(0, keyParts.Length)];
        }

        private static byte[] GetBody(string routingKey)
        {
            return Encoding.UTF8.GetBytes(routingKey + " " + DateTime.Now.ToLongTimeString());
        }
    }
}