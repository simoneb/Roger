using System;
using System.Text;
using System.Threading;
using ZMQ;

namespace ZeroMQ_Guide.Guide
{
    public class ReqRepBroker : Runnable
    {
        public override void Run()
        {
            Run(Broker);

            Thread.Sleep(100);

            Run(Client);
            Run(Server);
        }

        private static void Server()
        {
            using (var context = new Context(1))
            using (var server = context.Socket(SocketType.REP))
            {
                server.Connect("tcp://localhost:5560");

                while (true)
                {
                    Console.WriteLine("Server received: {0}", server.Recv(Encoding.UTF8));

                    Thread.Sleep(1000);

                    server.Send("World", Encoding.UTF8);
                }
            }
        }

        private static void Client()
        {
            using (var context = new Context(1))
            using (var client = context.Socket(SocketType.REQ))
            {
                client.Connect("tcp://localhost:5559");

                for (int i = 0; i < 10; i++)
                {
                    client.Send("Hello", Encoding.UTF8);

                    Console.WriteLine("Client received: {0}", client.Recv(Encoding.UTF8));
                }
            }
        }

        protected virtual void Broker()
        {
            using (var context = new Context(1))
            using (var frontend = context.Socket(SocketType.XREP))
            using (var backend = context.Socket(SocketType.XREQ))
            {
                frontend.Bind("tcp://*:5559");
                backend.Bind("tcp://*:5560");

                var fPoll = frontend.CreatePollItem(IOMultiPlex.POLLIN);
                fPoll.PollInHandler += delegate { PollOnPollInHandler(frontend, backend); };

                var bPoll = backend.CreatePollItem(IOMultiPlex.POLLIN);
                bPoll.PollInHandler += delegate { PollOnPollInHandler(backend, frontend); };

                var pollItems = new[] {fPoll, bPoll};

                while (true)
                    context.Poll(pollItems);
            }
        }

        private static void PollOnPollInHandler(Socket socket, Socket recipient)
        {
            bool isProcessing = true;
            while (isProcessing)
            {
                byte[] message = socket.Recv();
                recipient.Send(message, socket.RcvMore ? SendRecvOpt.SNDMORE : 0);
                isProcessing = socket.RcvMore;
            }
        }
    }
}