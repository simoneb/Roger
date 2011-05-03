using System;
using System.Text;
using System.Threading;
using ZMQ;

namespace ZeroMQ_Guide.Guide
{
    public class VentilatorWithKillSignal : Ventilator
    {
        protected override void Worker(int repetitionNumber)
        {
            using (var context = new Context(1))
            using (var receiver = context.Socket(SocketType.PULL))
            using (var sender = context.Socket(SocketType.PUSH))
            using (var controller = context.Socket(SocketType.SUB))
            {
                receiver.Connect("tcp://localhost:5557");
                sender.Connect("tcp://localhost:5558");
                controller.Connect("tcp://localhost:5559");
                
                controller.Subscribe("", Encoding.UTF8);

                var receiverPoll = receiver.CreatePollItem(IOMultiPlex.POLLIN);
                receiverPoll.PollInHandler += (socket, revents) => ReceiverPollOnPollInHandler(socket, sender);

                var controllerPoll = controller.CreatePollItem(IOMultiPlex.POLLIN);
                
                var run = true;
                controllerPoll.PollInHandler += (socket, revents) => run = false;

                Console.WriteLine("Worker {0} ready to receive", repetitionNumber);

                while (run)
                    context.Poll(new[] {receiverPoll, controllerPoll});

                Console.WriteLine("Worker {0} received kill signal", repetitionNumber);
            }
        }

        private static void ReceiverPollOnPollInHandler(Socket receiver, Socket sender)
        {
            var message = receiver.Recv(Encoding.UTF8);

            Thread.Sleep(int.Parse(message));

            sender.Send("ignored", Encoding.UTF8);
        }

        protected override void SinkImpl(Context context)
        {
            using (var controller = context.Socket(SocketType.PUB))
            {
                controller.Bind("tcp://*:5559");

                base.SinkImpl(context);

                controller.Send("KILL", Encoding.UTF8);

                Thread.Sleep(1000); // give ZMQ time to deliver
            }
        }
    }
}