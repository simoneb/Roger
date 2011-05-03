using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using ZMQ;

namespace ZeroMQ_Guide.Guide
{
    public class Ventilator : Runnable
    {
        public override void Run()
        {
            EventWaitHandle handle = new ManualResetEvent(false);
            Run(() => Publisher(handle));


            Thread.Sleep(100);

            Run(Worker, 10);

            Console.WriteLine("Press enter when workers are ready");
            Console.ReadLine();
            handle.Set();
            Console.WriteLine("Sending tasks to workers...");

            Run(Sink);
        }

        public static void Publisher()
        {
            Publisher(new ManualResetEvent(true));
        }

        public static void Publisher(WaitHandle handle)
        {
            using (var context = new Context(1))
            using (var sender = context.Socket(SocketType.PUSH))
            {
                sender.Bind("tcp://*:5557");

                handle.WaitOne();

                sender.Send("0", Encoding.UTF8);

                var rnd = new Random();

                var totalMs = 0;

                for (int taskNo = 0; taskNo < 100; taskNo++)
                {
                    int workload = rnd.Next(1, 100);
                    totalMs += workload;

                    sender.Send(workload.ToString(), Encoding.UTF8);
                }

                Console.WriteLine("Total expected cost: {0} msec", totalMs);
            }
        }

        private static void Worker(int number)
        {
            using (var context = new Context(1))
            using(var receiver = context.Socket(SocketType.PULL))
            using(var sender = context.Socket(SocketType.PUSH))
            {
                receiver.Connect("tcp://localhost:5557");
                sender.Connect("tcp://localhost:5558");

                Console.WriteLine(string.Format("Worker {0} ready to receive", number));

                while (true)
                {
                    var message = receiver.Recv(Encoding.UTF8);

                    Thread.Sleep(int.Parse(message));

                    sender.Send("", Encoding.UTF8);
                }
            }
        }

        private static void Sink()
        {
            using (var context = new Context(1))
            using (var receiver = context.Socket(SocketType.PULL))
            {
                receiver.Bind("tcp://*:5558");

                receiver.Recv(Encoding.UTF8);

                var watch = Stopwatch.StartNew();

                for (int taskNo = 0; taskNo < 100; taskNo++)
                {
                    receiver.Recv(Encoding.UTF8);

                    if((taskNo/10)*10 == taskNo)
                        Console.Write(":");
                    else
                        Console.Write(".");
                }

                Console.WriteLine("Total elapsed time: {0} msec", watch.ElapsedMilliseconds);
            }
        }
    }
}