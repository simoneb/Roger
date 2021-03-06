﻿using System;
using System.Text;
using System.Threading;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Tutorial3
{
    [Serializable]
    public class Consumer : IProcess
    {
        public void Start(WaitHandle waitHandle)
        {
            using (var connection = Helpers.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(Constants.ExchangeName, ExchangeType.Fanout, false, true, null);
                var queue = channel.QueueDeclare("", false, true, true, null);

                channel.QueueBind(queue, Constants.ExchangeName, "");

                var consumer = new EventingBasicConsumer {Model = channel};

                consumer.Received += ConsumerOnReceived;

                channel.BasicConsume(queue, true, consumer);

                waitHandle.WaitOne();
            }
        }

        private void ConsumerOnReceived(IBasicConsumer sender, BasicDeliverEventArgs args)
        {
            Console.WriteLine("Message received: {0}", Encoding.UTF8.GetString(args.Body));
        }
    }
}