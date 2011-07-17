using System;
using System.Collections;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Tests.Bus
{
    internal class FakeConnection : IConnection
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IModel CreateModel()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Close(ushort reasonCode, string reasonText)
        {
            throw new NotImplementedException();
        }

        public void Close(int timeout)
        {
            throw new NotImplementedException();
        }

        public void Close(ushort reasonCode, string reasonText, int timeout)
        {
            throw new NotImplementedException();
        }

        public void Abort()
        {
            throw new NotImplementedException();
        }

        public void Abort(ushort reasonCode, string reasonText)
        {
            throw new NotImplementedException();
        }

        public void Abort(int timeout)
        {
            throw new NotImplementedException();
        }

        public void Abort(ushort reasonCode, string reasonText, int timeout)
        {
            throw new NotImplementedException();
        }

        public AmqpTcpEndpoint Endpoint
        {
            get { throw new NotImplementedException(); }
        }

        public IProtocol Protocol
        {
            get { throw new NotImplementedException(); }
        }

        public ushort ChannelMax
        {
            get { throw new NotImplementedException(); }
        }

        public uint FrameMax
        {
            get { throw new NotImplementedException(); }
        }

        public ushort Heartbeat
        {
            get { throw new NotImplementedException(); }
        }

        public IDictionary ClientProperties
        {
            get { throw new NotImplementedException(); }
        }

        public IDictionary ServerProperties
        {
            get { throw new NotImplementedException(); }
        }

        public AmqpTcpEndpoint[] KnownHosts
        {
            get { throw new NotImplementedException(); }
        }

        public ShutdownEventArgs CloseReason
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsOpen
        {
            get { throw new NotImplementedException(); }
        }

        public bool AutoClose
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public IList ShutdownReport
        {
            get { throw new NotImplementedException(); }
        }

        public event ConnectionShutdownEventHandler ConnectionShutdown;
        public event CallbackExceptionEventHandler CallbackException;
    }
}