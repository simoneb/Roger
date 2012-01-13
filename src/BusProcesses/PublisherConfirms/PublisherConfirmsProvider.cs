using System.Collections.Generic;
using Common;
using RabbitMQ.Client;
using Roger;

namespace BusProcesses.PublisherConfirms
{
    public class PublisherConfirmsProvider : IProcessesProvider
    {
        public IEnumerable<IProcess> Processes
        {
            get
            {
                yield return new Producer();
                yield return new Consumer();
            }
        }

        public static void DeclareExchange(IConnectionFactory connectionFactory)
        {
            using (var connection = connectionFactory.CreateConnection())
            using (var model = connection.CreateModel())
                model.ExchangeDeclare("PublisherConfirms", ExchangeType.Topic);
        }
    }
}