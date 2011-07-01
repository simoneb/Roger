using System;
using Common;
using RabbitMQ.Client.Events;

namespace Tests.Utils
{
    public static class MessageExtensions
    {
        public const string MessageIdHeader = "X-MessageId";
        public const string SourceHeader = "X-Source";

        public static Guid Id(this BasicDeliverEventArgs message)
        {
            return Guid.Parse(((byte[])message.BasicProperties.Headers[MessageIdHeader]).String());
        }

        public static string Source(this BasicDeliverEventArgs message)
        {
            return ((byte[])message.BasicProperties.Headers[SourceHeader]).String();
        }
    }
}