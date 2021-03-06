using System.Collections;
using System.IO;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Common
{
    internal class SafeShutdownConnection : IConnection
    {
        private readonly IConnection inner;

        public SafeShutdownConnection(IConnection inner)
        {
            this.inner = inner;
        }

        public void Dispose()
        {
            try
            {
                inner.Dispose();
            }
            catch (OperationInterruptedException) { }
            catch (IOException) { }
        }

        public IModel CreateModel()
        {
            return inner.CreateModel();
        }

        public void Close()
        {
            inner.Close();
        }

        public void Close(ushort reasonCode, string reasonText)
        {
            inner.Close(reasonCode, reasonText);
        }

        public void Close(int timeout)
        {
            inner.Close(timeout);
        }

        public void Close(ushort reasonCode, string reasonText, int timeout)
        {
            inner.Close(reasonCode, reasonText, timeout);
        }

        public void Abort()
        {
            inner.Abort();
        }

        public void Abort(ushort reasonCode, string reasonText)
        {
            inner.Abort(reasonCode, reasonText);
        }

        public void Abort(int timeout)
        {
            inner.Abort(timeout);
        }

        public void Abort(ushort reasonCode, string reasonText, int timeout)
        {
            inner.Abort(reasonCode, reasonText, timeout);
        }

        public AmqpTcpEndpoint Endpoint
        {
            get { return inner.Endpoint; }
        }

        public IProtocol Protocol
        {
            get { return inner.Protocol; }
        }

        public ushort ChannelMax
        {
            get { return inner.ChannelMax; }
        }

        public uint FrameMax
        {
            get { return inner.FrameMax; }
        }

        public ushort Heartbeat
        {
            get { return inner.Heartbeat; }
        }

        public IDictionary ClientProperties
        {
            get { return inner.ClientProperties; }
        }

        public IDictionary ServerProperties
        {
            get { return inner.ServerProperties; }
        }

        public AmqpTcpEndpoint[] KnownHosts
        {
            get { return inner.KnownHosts; }
        }

        public ShutdownEventArgs CloseReason
        {
            get { return inner.CloseReason; }
        }

        public bool IsOpen
        {
            get { return inner.IsOpen; }
        }

        public bool AutoClose
        {
            get { return inner.AutoClose; }
            set { inner.AutoClose = value; }
        }

        public IList ShutdownReport
        {
            get { return inner.ShutdownReport; }
        }

        public event ConnectionShutdownEventHandler ConnectionShutdown
        {
            add { inner.ConnectionShutdown += value; }
            remove { inner.ConnectionShutdown -= value; }
        }

        public event CallbackExceptionEventHandler CallbackException
        {
            add { inner.CallbackException += value; }
            remove { inner.CallbackException -= value; }
        }
    }
}