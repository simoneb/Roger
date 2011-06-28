using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Shoveling.Test.Observable
{
    public class RabbitObservable : IRabbitObservable
    {
        private readonly ExchangeOptions m_exchange;
        private readonly IConnection m_connection;

        public RabbitObservable(ExchangeOptions exchange)
        {
            m_exchange = exchange;

            var connectionFactory = new ConnectionFactory();
            m_connection = connectionFactory.CreateConnection();
        }

        public IDisposable Subscribe(IObserver<RabbitMessage> observer)
        {
            return Subscribe(observer,  new QueueOptions());
        }

        public IDisposable Subscribe(IObserver<RabbitMessage> observer, QueueOptions queue = default(QueueOptions))
        {
            var model = m_connection.CreateModel();
            var queueName = model.QueueDeclare(queue.Name ?? string.Empty, false, true, true, null);

            Bind(observer, model, queueName);

            var consumer = new QueueingBasicConsumer(model);

            var consumerTag = model.BasicConsume(queueName, true, consumer);

            foreach (BasicDeliverEventArgs message in consumer.Queue)
            {
                try
                {
                    observer.OnNext(new RabbitMessage(message));
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                }
            }

            return Disposable.Create(model.Dispose);
        }

        private void Bind(IObserver<RabbitMessage> observer, IModel model, string queue)
        {
            var bindings = new Collection<string>();
            var cast = observer as IRabbitObserver;

            if (cast != null)
                cast.AddBindingsTo(bindings);

            var bound = false;

            foreach (var bindingKey in bindings)
            {
                bound = true;
                model.QueueBind(queue, m_exchange.Name, bindingKey);
            }

            if(!bound)
                model.QueueBind(queue, m_exchange.Name, "#");
        }
    }
}