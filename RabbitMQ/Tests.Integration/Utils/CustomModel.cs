using System.Collections;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Tests.Integration.Utils
{
    public class CustomModel : ICustomModel
    {
        private readonly IModel inner;

        public CustomModel(IConnection connection)
        {
            inner = connection.CreateModel();
        }

        public void Dispose()
        {
            inner.Dispose();
            Disposed = true;
        }

        public bool Disposed { get; private set; }

        public void WaitForConfirmsOrDie()
        {
            inner.WaitForConfirmsOrDie();
        }

        public bool WaitForConfirms()
        {
            return inner.WaitForConfirms();
        }

        public IBasicProperties CreateBasicProperties()
        {
            return inner.CreateBasicProperties();
        }

        public IFileProperties CreateFileProperties()
        {
            return inner.CreateFileProperties();
        }

        public IStreamProperties CreateStreamProperties()
        {
            return inner.CreateStreamProperties();
        }

        public void ChannelFlow(bool active)
        {
            inner.ChannelFlow(active);
        }

        public void ExchangeDeclare(string exchange, string type, bool durable, bool autoDelete, IDictionary arguments)
        {
            inner.ExchangeDeclare(exchange, type, durable, autoDelete, arguments);
        }

        public void ExchangeDeclare(string exchange, string type, bool durable)
        {
            inner.ExchangeDeclare(exchange, type, durable);
        }

        public void ExchangeDeclare(string exchange, string type)
        {
            inner.ExchangeDeclare(exchange, type);
        }

        public void ExchangeDeclarePassive(string exchange)
        {
            inner.ExchangeDeclarePassive(exchange);
        }

        public void ExchangeDelete(string exchange, bool ifUnused)
        {
            inner.ExchangeDelete(exchange, ifUnused);
        }

        public void ExchangeDelete(string exchange)
        {
            inner.ExchangeDelete(exchange);
        }

        public void ExchangeBind(string destination, string source, string routingKey, IDictionary arguments)
        {
            inner.ExchangeBind(destination, source, routingKey, arguments);
        }

        public void ExchangeBind(string destination, string source, string routingKey)
        {
            inner.ExchangeBind(destination, source, routingKey);
        }

        public void ExchangeUnbind(string destination, string source, string routingKey, IDictionary arguments)
        {
            inner.ExchangeUnbind(destination, source, routingKey, arguments);
        }

        public void ExchangeUnbind(string destination, string source, string routingKey)
        {
            inner.ExchangeUnbind(destination, source, routingKey);
        }

        public QueueDeclareOk QueueDeclare()
        {
            return inner.QueueDeclare();
        }

        public QueueDeclareOk QueueDeclarePassive(string queue)
        {
            return inner.QueueDeclarePassive(queue);
        }

        public QueueDeclareOk QueueDeclare(string queue, bool durable, bool exclusive, bool autoDelete, IDictionary arguments)
        {
            return inner.QueueDeclare(queue, durable, exclusive, autoDelete, arguments);
        }

        public void QueueBind(string queue, string exchange, string routingKey, IDictionary arguments)
        {
            inner.QueueBind(queue, exchange, routingKey, arguments);
        }

        public void QueueBind(string queue, string exchange, string routingKey)
        {
            inner.QueueBind(queue, exchange, routingKey);
        }

        public void QueueUnbind(string queue, string exchange, string routingKey, IDictionary arguments)
        {
            inner.QueueUnbind(queue, exchange, routingKey, arguments);
        }

        public uint QueuePurge(string queue)
        {
            return inner.QueuePurge(queue);
        }

        public uint QueueDelete(string queue, bool ifUnused, bool ifEmpty)
        {
            return inner.QueueDelete(queue, ifUnused, ifEmpty);
        }

        public uint QueueDelete(string queue)
        {
            return inner.QueueDelete(queue);
        }

        public void ConfirmSelect()
        {
            inner.ConfirmSelect();
        }

        public string BasicConsume(string queue, bool noAck, IBasicConsumer consumer)
        {
            return inner.BasicConsume(queue, noAck, consumer);
        }

        public string BasicConsume(string queue, bool noAck, string consumerTag, IBasicConsumer consumer)
        {
            return inner.BasicConsume(queue, noAck, consumerTag, consumer);
        }

        public string BasicConsume(string queue, bool noAck, string consumerTag, IDictionary arguments, IBasicConsumer consumer)
        {
            return inner.BasicConsume(queue, noAck, consumerTag, arguments, consumer);
        }

        public string BasicConsume(string queue, bool noAck, string consumerTag, bool noLocal, bool exclusive, IDictionary arguments, IBasicConsumer consumer)
        {
            return inner.BasicConsume(queue, noAck, consumerTag, noLocal, exclusive, arguments, consumer);
        }

        public void BasicCancel(string consumerTag)
        {
            inner.BasicCancel(consumerTag);
        }

        public void BasicQos(uint prefetchSize, ushort prefetchCount, bool global)
        {
            inner.BasicQos(prefetchSize, prefetchCount, global);
        }

        public void BasicPublish(PublicationAddress addr, IBasicProperties basicProperties, byte[] body)
        {
            inner.BasicPublish(addr, basicProperties, body);
        }

        public void BasicPublish(string exchange, string routingKey, IBasicProperties basicProperties, byte[] body)
        {
            inner.BasicPublish(exchange, routingKey, basicProperties, body);
        }

        public void BasicPublish(string exchange, string routingKey, bool mandatory, bool immediate, IBasicProperties basicProperties, byte[] body)
        {
            inner.BasicPublish(exchange, routingKey, mandatory, immediate, basicProperties, body);
        }

        public void BasicAck(ulong deliveryTag, bool multiple)
        {
            inner.BasicAck(deliveryTag, multiple);
        }

        public void BasicReject(ulong deliveryTag, bool requeue)
        {
            inner.BasicReject(deliveryTag, requeue);
        }

        public void BasicNack(ulong deliveryTag, bool multiple, bool requeue)
        {
            inner.BasicNack(deliveryTag, multiple, requeue);
        }

        public void BasicRecover(bool requeue)
        {
            inner.BasicRecover(requeue);
        }

        public void BasicRecoverAsync(bool requeue)
        {
            inner.BasicRecoverAsync(requeue);
        }

        public BasicGetResult BasicGet(string queue, bool noAck)
        {
            return inner.BasicGet(queue, noAck);
        }

        public void TxSelect()
        {
            inner.TxSelect();
        }

        public void TxCommit()
        {
            inner.TxCommit();
        }

        public void TxRollback()
        {
            inner.TxRollback();
        }

        public void DtxSelect()
        {
            inner.DtxSelect();
        }

        public void DtxStart(string dtxIdentifier)
        {
            inner.DtxStart(dtxIdentifier);
        }

        public void Close()
        {
            inner.Close();
        }

        public void Close(ushort replyCode, string replyText)
        {
            inner.Close(replyCode, replyText);
        }

        public void Abort()
        {
            inner.Abort();
        }

        public void Abort(ushort replyCode, string replyText)
        {
            inner.Abort(replyCode, replyText);
        }

        public IBasicConsumer DefaultConsumer
        {
            get { return inner.DefaultConsumer; }
            set { inner.DefaultConsumer = value; }
        }

        public ShutdownEventArgs CloseReason
        {
            get { return inner.CloseReason; }
        }

        public bool IsOpen
        {
            get { return inner.IsOpen; }
        }

        public ulong NextPublishSeqNo
        {
            get { return inner.NextPublishSeqNo; }
        }

        public event ModelShutdownEventHandler ModelShutdown
        {
            add { inner.ModelShutdown += value; }
            remove { inner.ModelShutdown -= value; }
        }

        public event BasicReturnEventHandler BasicReturn
        {
            add { inner.BasicReturn += value; }
            remove { inner.BasicReturn -= value; }
        }

        public event BasicAckEventHandler BasicAcks
        {
            add { inner.BasicAcks += value; }
            remove { inner.BasicAcks -= value; }
        }

        public event BasicNackEventHandler BasicNacks
        {
            add { inner.BasicNacks += value; }
            remove { inner.BasicNacks -= value; }
        }

        public event CallbackExceptionEventHandler CallbackException
        {
            add { inner.CallbackException += value; }
            remove { inner.CallbackException -= value; }
        }

        public event FlowControlEventHandler FlowControl
        {
            add { inner.FlowControl += value; }
            remove { inner.FlowControl -= value; }
        }

        public event BasicRecoverOkEventHandler BasicRecoverOk
        {
            add { inner.BasicRecoverOk += value; }
            remove { inner.BasicRecoverOk -= value; }
        }
    }
}