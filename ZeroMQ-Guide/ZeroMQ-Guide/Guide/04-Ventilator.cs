using System;
using System.Text;
using System.Threading;
using ZMQ;

namespace ZeroMQ_Guide.Guide
{
    public class Ventilator : Runnable
    {
        public override void Run()
        {
            Run(Worker, 10);
            Run(VentilatorMethod);
        }

        private static void VentilatorMethod()
        {
            using (var context = new Context(1))
            using (var sender = context.Socket(SocketType.PUSH))
            {
                sender.Bind("tcp://*.5557");

                Console.WriteLine("Press enter when workers are ready");
                Console.ReadLine();
                Console.WriteLine("Sending tasks to workers...");

                var rnd = new Random();

                var totalMs = 0;

                for (int taskNo = 0; taskNo < 100; taskNo++)
                {
                    int workload = rnd.Next(100) + 1;
                    totalMs += workload;

                    sender.Send(workload.ToString(), Encoding.UTF8);
                }

                Console.WriteLine("Total expected cost: {0} msec", totalMs);

                Thread.Sleep(10);
            }
        }

        private void Worker()
        {
            new Context();
        }
    }
}