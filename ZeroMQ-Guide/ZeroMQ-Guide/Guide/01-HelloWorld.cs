using System;
using System.Text;
using System.Threading;
using ZMQ;

namespace ZeroMQ_Guide.Guide
{
    public class HelloWorld : Runnable
    {
        public override void Run()
        {
            Run(Client);
            Run(Server);
        }

        private static void Client()
        {
            using (var context = new Context(1))
                Client(context);
        }

        public static void Client(Context context)
        {
            using (var requester = context.Socket(SocketType.REQ))
            {
                requester.Connect("tcp://localhost:5555");

                var requestNo = 0;
                while (requestNo++ < 10)
                {
                    requester.Send("Hello", Encoding.UTF8);

                    Console.WriteLine("Client received {0}", requester.Recv(Encoding.UTF8));
                }
            }
        }

        private static void Server()
        {
            using (var context = new Context(1))
            {
                using (var responder = context.Socket(SocketType.REP))
                {
                    responder.Bind("tcp://*:5555");

                    while (true)
                    {
                        var message = responder.Recv(Encoding.UTF8);

                        Console.WriteLine("Server received {0}", message);

                        Thread.Sleep(1000);

                        responder.Send("World", Encoding.UTF8);
                    }
                }
            }
        }
    }
}