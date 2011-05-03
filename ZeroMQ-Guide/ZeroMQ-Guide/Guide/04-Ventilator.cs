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
            Run(Publisher, handle);

            // let publisher bind its socket
            Thread.Sleep(100);

            Run(10, Worker);

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

        private static void Publisher(WaitHandle handle)
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

        protected virtual void Worker(int repetitionNumber)
        {
            using (var context = new Context(1))
            using(var receiver = context.Socket(SocketType.PULL))
            using(var sender = context.Socket(SocketType.PUSH))
            {
                receiver.Connect("tcp://localhost:5557");
                sender.Connect("tcp://localhost:5558");

                Console.WriteLine("Worker {0} ready to receive", repetitionNumber);

                while (true)
                {
                    var message = receiver.Recv(Encoding.UTF8);

                    Thread.Sleep(int.Parse(message));

                    sender.Send("ignored", Encoding.UTF8);
                }
            }
        }

        private void Sink()
        {
            using (var context = new Context(1))
            SinkImpl(context);
        }

        protected virtual void SinkImpl(Context context)
        {
            using (var receiver = context.Socket(SocketType.PULL))
            {
                receiver.Bind("tcp://*:5558");

                receiver.Recv();

                var watch = Stopwatch.StartNew();

                for (int taskNo = 0; taskNo < 100; taskNo++)
                {
                    receiver.Recv();

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