using System;
using Common;
using RabbitMQ.Client.Events;

namespace Shoveling.Test.Utils
{
    public static class MessageExtensions
    {
        public const string MessageIdHeader = "X-MessageId";

        public static Guid Id(this BasicDeliverEventArgs message)
        {
            return Guid.Parse(((byte[])message.BasicProperties.Headers[MessageIdHeader]).String());
        }
    }
}