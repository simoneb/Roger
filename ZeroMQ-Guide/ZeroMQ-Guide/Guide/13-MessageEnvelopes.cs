using System;
using System.Text;
using System.Threading;
using ZeroMQExtensions;
using ZMQ;

namespace ZeroMQ_Guide.Guide
{
    public class MessageEnvelopes : Runnable
    {
        public override void Run()
        {
            Run(Publisher);
            Run(Subscriber);
        }

        private static void Publisher()
        {
            using (var context = new Context(1))
            using (var socket = context.Pub().Bind("tcp://*:5563"))
            {
                while (true)
                {
                    socket.SendMore("A", Encoding.UTF8);
                    socket.Send("We don't want to see this", Encoding.UTF8);
                    socket.SendMore("B", Encoding.UTF8);
                    socket.Send("We want to see this", Encoding.UTF8);

                    Thread.Sleep(1000);
                }
            }
        }

        private static void Subscriber()
        {
            using (var context = new Context(1))
            using (var socket = context.Sub().Connect("tcp://localhost:5563").Subscribe("B"))
            {
                while (true)
                {
                    var address = socket.Recv(Encoding.UTF8);
                    var contents = socket.Recv(Encoding.UTF8);

                    Console.WriteLine("[{0}] {1}", address, contents);
                }
            }
        }
    }
}