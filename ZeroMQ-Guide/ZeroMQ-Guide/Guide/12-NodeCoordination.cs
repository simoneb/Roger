using System;
using System.Text;
using System.Threading;
using ZMQ;

namespace ZeroMQ_Guide.Guide
{
    public class NodeCoordination : Runnable
    {
        public override void Run()
        {
            Run(Publisher, 10);
            Run(10, Subscriber);
        }

        private static void Subscriber(int subscriberNumber)
        {
            using (var context = new Context(1))
            using (var coordinator = context.Socket(SocketType.REQ))
            using(var sub = context.Socket(SocketType.SUB))
            {
                sub.Connect("tcp://localhost:5562");
                sub.Subscribe("", Encoding.UTF8);
                coordinator.Connect("tcp://localhost:5561");

                Thread.Sleep(100);

                coordinator.Send();
                coordinator.Recv();

                int received = 0;
                while (sub.Recv(Encoding.UTF8) != "END")
                    received++;

                Console.WriteLine("Subscriber {0} received {1} messages", subscriberNumber, received);
            }
        }

        private static void Publisher(int numberOfSubscribers)
        {
            using (var context = new Context(1))
            using (var coordinator = context.Socket(SocketType.REP))
            using(var pub = context.Socket(SocketType.PUB))
            {
                coordinator.Bind("tcp://*:5561");
                pub.Bind("tcp://*:5562");

                int subscriptions = 0;
                while(subscriptions++ < numberOfSubscribers)
                {
                    coordinator.Recv();
                    Console.WriteLine("Received subscription");
                    coordinator.Send();
                }

                Console.WriteLine("Starting to send messages");

                for (int i = 0; i < 1000000; i++)
                    pub.Send("Robhart", Encoding.UTF8);

                pub.Send("END", Encoding.UTF8);
            }
        }
    }
}