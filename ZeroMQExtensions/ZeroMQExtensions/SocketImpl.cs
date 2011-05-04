using System;
using System.Collections.Generic;
using System.Text;
using ZMQ;

namespace ZeroMQExtensions
{
    internal abstract class SocketImpl : ISocket
    {
        protected readonly Socket Socket;

        protected SocketImpl(Socket socket)
        {
            Socket = socket;
        }

        public void Dispose()
        {
            Socket.Dispose();
        }

        public PollItem CreatePollItem(IOMultiPlex events)
        {
            return Socket.CreatePollItem(events);
        }

        public PollItem CreatePollItem(IOMultiPlex events, System.Net.Sockets.Socket sysSocket)
        {
            return Socket.CreatePollItem(events, sysSocket);
        }

        public void SetSockOpt(SocketOpt option, ulong value)
        {
            Socket.SetSockOpt(option, value);
        }

        public void SetSockOpt(SocketOpt option, byte[] value)
        {
            Socket.SetSockOpt(option, value);
        }

        public void SetSockOpt(SocketOpt option, int value)
        {
            Socket.SetSockOpt(option, value);
        }

        public void SetSockOpt(SocketOpt option, long value)
        {
            Socket.SetSockOpt(option, value);
        }

        public object GetSockOpt(SocketOpt option)
        {
            return Socket.GetSockOpt(option);
        }

        public void Bind(string addr)
        {
            Socket.Bind(addr);
        }

        public void Bind(Transport transport, string addr, uint port)
        {
            Socket.Bind(transport, addr, port);
        }

        public void Bind(Transport transport, string addr)
        {
            Socket.Bind(transport, addr);
        }

        public void Connect(string addr)
        {
            Socket.Connect(addr);
        }

        public void Forward(Socket destination)
        {
            Socket.Forward(destination);
        }

        public byte[] Recv(params SendRecvOpt[] flags)
        {
            return Socket.Recv(flags);
        }

        public byte[] Recv()
        {
            return Socket.Recv();
        }

        public byte[] Recv(int timeout)
        {
            return Socket.Recv(timeout);
        }

        public string Recv(Encoding encoding)
        {
            return Socket.Recv(encoding);
        }

        public string Recv(Encoding encoding, int timeout)
        {
            return Socket.Recv(encoding, timeout);
        }

        public string Recv(Encoding encoding, params SendRecvOpt[] flags)
        {
            return Socket.Recv(encoding, flags);
        }

        public Queue<byte[]> RecvAll()
        {
            return Socket.RecvAll();
        }

        public Queue<byte[]> RecvAll(params SendRecvOpt[] flags)
        {
            return Socket.RecvAll(flags);
        }

        public Queue<string> RecvAll(Encoding encoding)
        {
            return Socket.RecvAll(encoding);
        }

        public Queue<string> RecvAll(Encoding encoding, params SendRecvOpt[] flags)
        {
            return Socket.RecvAll(encoding, flags);
        }

        public void Send(byte[] message, params SendRecvOpt[] flags)
        {
            Socket.Send(message, flags);
        }

        public void Send(byte[] message)
        {
            Socket.Send(message);
        }

        public void Send(string message, Encoding encoding)
        {
            Socket.Send(message, encoding);
        }

        public void Send()
        {
            Socket.Send();
        }

        public void SendMore()
        {
            Socket.SendMore();
        }

        public void SendMore(byte[] message)
        {
            Socket.SendMore(message);
        }

        public void SendMore(string message, Encoding encoding)
        {
            Socket.SendMore(message, encoding);
        }

        public void SendMore(string message, Encoding encoding, params SendRecvOpt[] flags)
        {
            Socket.SendMore(message, encoding, flags);
        }

        public void Send(string message, Encoding encoding, params SendRecvOpt[] flags)
        {
            Socket.Send(message, encoding, flags);
        }

        public string IdentityToString(Encoding encoding)
        {
            return Socket.IdentityToString(encoding);
        }

        public void StringToIdentity(string identity, Encoding encoding)
        {
            Socket.StringToIdentity(identity, encoding);
        }

        public byte[] Identity
        {
            get { return Socket.Identity; }
            set { Socket.Identity = value; }
        }

        public ulong HWM
        {
            get { return Socket.HWM; }
            set { Socket.HWM = value; }
        }

        public long Swap
        {
            get { return Socket.Swap; }
            set { Socket.Swap = value; }
        }

        public bool RcvMore
        {
            get { return Socket.RcvMore; }
        }

        public ulong Affinity
        {
            get { return Socket.Affinity; }
            set { Socket.Affinity = value; }
        }

        public long Rate
        {
            get { return Socket.Rate; }
            set { Socket.Rate = value; }
        }

        public long RecoveryIvl
        {
            get { return Socket.RecoveryIvl; }
            set { Socket.RecoveryIvl = value; }
        }

        public long MCastLoop
        {
            get { return Socket.MCastLoop; }
            set { Socket.MCastLoop = value; }
        }

        public ulong SndBuf
        {
            get { return Socket.SndBuf; }
            set { Socket.SndBuf = value; }
        }

        public ulong RcvBuf
        {
            get { return Socket.RcvBuf; }
            set { Socket.RcvBuf = value; }
        }

        public int Linger
        {
            get { return Socket.Linger; }
            set { Socket.Linger = value; }
        }

        public int ReconnectIvl
        {
            get { return Socket.ReconnectIvl; }
            set { Socket.ReconnectIvl = value; }
        }

        public int Backlog
        {
            get { return Socket.Backlog; }
            set { Socket.Backlog = value; }
        }

        public IntPtr FD
        {
            get { return Socket.FD; }
        }

        public IOMultiPlex[] Events
        {
            get { return Socket.Events; }
        }

        public string Address
        {
            get { return Socket.Address; }
        }

        public event PollHandler PollInHandler
        {
            add { Socket.PollInHandler += value; }
            remove { Socket.PollInHandler -= value; }
        }

        public event PollHandler PollOutHandler
        {
            add { Socket.PollOutHandler += value; }
            remove { Socket.PollOutHandler -= value; }
        }

        public event PollHandler PollErrHandler
        {
            add { Socket.PollErrHandler += value; }
            remove { Socket.PollErrHandler -= value; }
        }
    }
}