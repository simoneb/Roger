using System.Collections;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Roger.Internal.Impl
{
    internal class ModelWithConnectionDecorator : IModelWithConnection
    {
        public IReliableConnection Connection { get; private set; }
        private readonly IModel model;

        public ModelWithConnectionDecorator(IModel model, IReliableConnection connection)
        {
            Connection = connection;
            this.model = model;
        }

        public void Dispose()
        {
            model.Dispose();
        }

        public IBasicProperties CreateBasicProperties()
        {
            return model.CreateBasicProperties();
        }

        public IFileProperties CreateFileProperties()
        {
            return model.CreateFileProperties();
        }

        public IStreamProperties CreateStreamProperties()
        {
            return model.CreateStreamProperties();
        }

        public void ChannelFlow(bool active)
        {
            model.ChannelFlow(active);
        }

        public void ExchangeDeclare(string exchange, string type, bool durable, bool autoDelete, IDictionary arguments)
        {
            model.ExchangeDeclare(exchange, type, durable, autoDelete, arguments);
        }

        public void ExchangeDeclare(string exchange, string type, bool durable)
        {
            model.ExchangeDeclare(exchange, type, durable);
        }

        public void ExchangeDeclare(string exchange, string type)
        {
            model.ExchangeDeclare(exchange, type);
        }

        public void ExchangeDeclarePassive(string exchange)
        {
            model.ExchangeDeclarePassive(exchange);
        }

        public void ExchangeDelete(string exchange, bool ifUnused)
        {
            model.ExchangeDelete(exchange, ifUnused);
        }

        public void ExchangeDelete(string exchange)
        {
            model.ExchangeDelete(exchange);
        }

        public void ExchangeBind(string destination, string source, string routingKey, IDictionary arguments)
        {
            model.ExchangeBind(destination, source, routingKey, arguments);
        }

        public void ExchangeBind(string destination, string source, string routingKey)
        {
            model.ExchangeBind(destination, source, routingKey);
        }

        public void ExchangeUnbind(string destination, string source, string routingKey, IDictionary arguments)
        {
            model.ExchangeUnbind(destination, source, routingKey, arguments);
        }

        public void ExchangeUnbind(string destination, string source, string routingKey)
        {
            model.ExchangeUnbind(destination, source, routingKey);
        }

        public QueueDeclareOk QueueDeclare()
        {
            return model.QueueDeclare();
        }

        public QueueDeclareOk QueueDeclarePassive(string queue)
        {
            return model.QueueDeclarePassive(queue);
        }

        public QueueDeclareOk QueueDeclare(string queue, bool durable, bool exclusive, bool autoDelete, IDictionary arguments)
        {
            return model.QueueDeclare(queue, durable, exclusive, autoDelete, arguments);
        }

        public void QueueBind(string queue, string exchange, string routingKey, IDictionary arguments)
        {
            model.QueueBind(queue, exchange, routingKey, arguments);
        }

        public void QueueBind(string queue, string exchange, string routingKey)
        {
            model.QueueBind(queue, exchange, routingKey);
        }

        public void QueueUnbind(string queue, string exchange, string routingKey, IDictionary arguments)
        {
            model.QueueUnbind(queue, exchange, routingKey, arguments);
        }

        public uint QueuePurge(string queue)
        {
            return model.QueuePurge(queue);
        }

        public uint QueueDelete(string queue, bool ifUnused, bool ifEmpty)
        {
            return model.QueueDelete(queue, ifUnused, ifEmpty);
        }

        public uint QueueDelete(string queue)
        {
            return model.QueueDelete(queue);
        }

        public void ConfirmSelect()
        {
            model.ConfirmSelect();
        }

        public bool WaitForConfirms()
        {
            return model.WaitForConfirms();
        }

        public void WaitForConfirmsOrDie()
        {
            model.WaitForConfirmsOrDie();
        }

        public string BasicConsume(string queue, bool noAck, IBasicConsumer consumer)
        {
            return model.BasicConsume(queue, noAck, consumer);
        }

        public string BasicConsume(string queue, bool noAck, string consumerTag, IBasicConsumer consumer)
        {
            return model.BasicConsume(queue, noAck, consumerTag, consumer);
        }

        public string BasicConsume(string queue, bool noAck, string consumerTag, IDictionary arguments, IBasicConsumer consumer)
        {
            return model.BasicConsume(queue, noAck, consumerTag, arguments, consumer);
        }

        public string BasicConsume(string queue, bool noAck, string consumerTag, bool noLocal, bool exclusive, IDictionary arguments, IBasicConsumer consumer)
        {
            return model.BasicConsume(queue, noAck, consumerTag, noLocal, exclusive, arguments, consumer);
        }

        public void BasicCancel(string consumerTag)
        {
            model.BasicCancel(consumerTag);
        }

        public void BasicQos(uint prefetchSize, ushort prefetchCount, bool global)
        {
            model.BasicQos(prefetchSize, prefetchCount, global);
        }

        public void BasicPublish(PublicationAddress addr, IBasicProperties basicProperties, byte[] body)
        {
            model.BasicPublish(addr, basicProperties, body);
        }

        public void BasicPublish(string exchange, string routingKey, IBasicProperties basicProperties, byte[] body)
        {
            model.BasicPublish(exchange, routingKey, basicProperties, body);
        }

        public void BasicPublish(string exchange, string routingKey, bool mandatory, bool immediate, IBasicProperties basicProperties, byte[] body)
        {
            model.BasicPublish(exchange, routingKey, mandatory, immediate, basicProperties, body);
        }

        public void BasicAck(ulong deliveryTag, bool multiple)
        {
            model.BasicAck(deliveryTag, multiple);
        }

        public void BasicReject(ulong deliveryTag, bool requeue)
        {
            model.BasicReject(deliveryTag, requeue);
        }

        public void BasicNack(ulong deliveryTag, bool multiple, bool requeue)
        {
            model.BasicNack(deliveryTag, multiple, requeue);
        }

        public void BasicRecover(bool requeue)
        {
            model.BasicRecover(requeue);
        }

        public void BasicRecoverAsync(bool requeue)
        {
            model.BasicRecoverAsync(requeue);
        }

        public BasicGetResult BasicGet(string queue, bool noAck)
        {
            return model.BasicGet(queue, noAck);
        }

        public void TxSelect()
        {
            model.TxSelect();
        }

        public void TxCommit()
        {
            model.TxCommit();
        }

        public void TxRollback()
        {
            model.TxRollback();
        }

        public void DtxSelect()
        {
            model.DtxSelect();
        }

        public void DtxStart(string dtxIdentifier)
        {
            model.DtxStart(dtxIdentifier);
        }

        public void Close()
        {
            model.Close();
        }

        public void Close(ushort replyCode, string replyText)
        {
            model.Close(replyCode, replyText);
        }

        public void Abort()
        {
            model.Abort();
        }

        public void Abort(ushort replyCode, string replyText)
        {
            model.Abort(replyCode, replyText);
        }

        public IBasicConsumer DefaultConsumer
        {
            get { return model.DefaultConsumer; }
            set { model.DefaultConsumer = value; }
        }

        public ShutdownEventArgs CloseReason
        {
            get { return model.CloseReason; }
        }

        public bool IsOpen
        {
            get { return model.IsOpen; }
        }

        public ulong NextPublishSeqNo
        {
            get { return model.NextPublishSeqNo; }
        }

        public event ModelShutdownEventHandler ModelShutdown
        {
            add { model.ModelShutdown += value; }
            remove { model.ModelShutdown -= value; }
        }

        public event BasicReturnEventHandler BasicReturn
        {
            add { model.BasicReturn += value; }
            remove { model.BasicReturn -= value; }
        }

        public event BasicAckEventHandler BasicAcks
        {
            add { model.BasicAcks += value; }
            remove { model.BasicAcks -= value; }
        }

        public event BasicNackEventHandler BasicNacks
        {
            add { model.BasicNacks += value; }
            remove { model.BasicNacks -= value; }
        }

        public event CallbackExceptionEventHandler CallbackException
        {
            add { model.CallbackException += value; }
            remove { model.CallbackException -= value; }
        }

        public event FlowControlEventHandler FlowControl
        {
            add { model.FlowControl += value; }
            remove { model.FlowControl -= value; }
        }

        public event BasicRecoverOkEventHandler BasicRecoverOk
        {
            add { model.BasicRecoverOk += value; }
            remove { model.BasicRecoverOk -= value; }
        }
    }
}