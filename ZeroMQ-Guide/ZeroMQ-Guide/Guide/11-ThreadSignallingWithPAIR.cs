using System;
using System.Text;
using ZMQ;

namespace ZeroMQ_Guide.Guide
{
    public class ThreadSignallingWithPAIR : Runnable
    {
        public override void Run()
        {
            using (var context = new Context(1))
            using (var receiver = context.Socket(SocketType.PAIR))
            {
                receiver.Bind("inproc://step3");

                Run(Step2, context);

                receiver.Recv();

                Console.WriteLine("Test successful!");
            }
        }

        private static void Step2(Context context)
        {
            using (var receiver = context.Socket(SocketType.PAIR))
            {
                receiver.Bind("inproc://step2");

                Run(Step1, context);

                receiver.Recv();
            }

            using (var xmitter = context.Socket(SocketType.PAIR))
            {
                xmitter.Connect("inproc://step3");
                xmitter.Send("READY", Encoding.UTF8);
            }
        }

        private static void Step1(Context context)
        {
            using (var receiver = context.Socket(SocketType.PAIR))
            {
                receiver.Connect("inproc://step2");
                receiver.Send("READY", Encoding.UTF8);
            }
        }
    }
}