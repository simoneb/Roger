using System;
using System.Text;
using System.Threading;
using ZeroMQExtensions;
using ZMQ;

namespace ZeroMQ_Guide.Guide
{
    public class DurablePubSub : Runnable
    {
        public override void Run()
        {
            Run(Publisher, 2ul);

            Restart(Run(Subscriber));

            Restart(Run(Subscriber));

            Run(Subscriber);
        }

        private static void Restart(CancellationTokenSource source)
        {
            Thread.Sleep(2000);
            source.Cancel();
            Console.WriteLine("Canceled subscriber");
            Thread.Sleep(3000);
            Console.WriteLine("Started subscriber");
        }

        private static void Publisher(ulong hwm)
        {
            using (var context = new Context(1))
            using (var sync = context.Pull().BoundTo("tcp://*:5564"))
            using (var publisher = context.Pub().BoundTo("tcp://*:5565").HighWatermark(hwm))
            {
                sync.Recv();

                for (int i = 0; i < 20; i++)
                {
                    publisher.Send(string.Format("Update {0}", i), Encoding.UTF8);
                    Thread.Sleep(500);
                }

                publisher.Send("END", Encoding.UTF8);

                Thread.Sleep(1000);
            }
        }

        private static void Subscriber(CancellationToken token)
        {
            using (var context = new Context(1))
            using (var sync = context.Push().ConnectedTo("tcp://localhost:5564"))
            using (var subscriber = context.Sub().WithIdentity("Hello").SubscribedToAnything().ConnectedTo("tcp://localhost:5565"))
            {
                sync.Send();

                string message = null;
                while (!token.IsCancellationRequested && (message = subscriber.Recv(Encoding.UTF8)) != "END")
                    Console.WriteLine(message);

                if(message == "END")
                    Console.WriteLine("Received END message");
            }
        }
    }
}