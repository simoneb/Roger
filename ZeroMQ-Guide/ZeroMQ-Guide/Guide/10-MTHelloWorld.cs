using System;
using System.Text;
using System.Threading;
using ZMQ;

namespace ZeroMQ_Guide.Guide
{
    public class MTHelloWorld : Runnable
    {
        public override void Run()
        {
            var context = new Context(1);

            Run(Queue, context);
            Run(HelloWorld.Client, context);

            // let zmq initialize inproc socket
            Thread.Sleep(10);

            Run(3, Worker, context);
        }

        private static void Queue(Context context)
        {
            using (var clients = context.Socket(SocketType.XREP))
            using (var workers = context.Socket(SocketType.XREQ))
            {
                clients.Bind("tcp://*:5555");
                workers.Bind("inproc://workers");

                Socket.Device.Queue(clients, workers);
            }
        }

        private static void Worker(int repetition, Context context)
        {
            var receiver = context.Socket(SocketType.REP);
            receiver.Connect("inproc://workers");

            while (true)
            {
                Console.WriteLine("Worker {0} received request {1}", repetition, receiver.Recv(Encoding.UTF8));

                Thread.Sleep(1000);

                receiver.Send("World", Encoding.UTF8);
            }
        }
    }
}