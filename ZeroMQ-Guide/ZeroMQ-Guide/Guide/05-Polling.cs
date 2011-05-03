using System;
using System.Text;
using ZMQ;

namespace ZeroMQ_Guide.Guide
{
    public class Polling : Runnable
    {
        public override void Run()
        {
            Run(Weather.Publisher);
            Run(Ventilator.Publisher);

            Run(Poller);
        }

        private static void Poller()
        {
            using (var context = new Context(1))
            {
                using (Socket receiver = context.Socket(SocketType.PULL), subscriber = context.Socket(SocketType.SUB))
                {
                    receiver.Connect("tcp://localhost:5557");

                    subscriber.Connect("tcp://localhost:5556");
                    subscriber.Subscribe("10001 ", Encoding.UTF8);

                    var receiverPoll = receiver.CreatePollItem(IOMultiPlex.POLLIN);
                    receiverPoll.PollInHandler += ReceiverPollOnPollInHandler;
                    var subscriberPoll = subscriber.CreatePollItem(IOMultiPlex.POLLIN);
                    subscriberPoll.PollInHandler += SubscriberPollOnPollInHandler;

                    var pollItems = new[] {receiverPoll, subscriberPoll};

                    while (true)
                    {
                        context.Poll(pollItems);
                    }
                }
            }
        }

        private static void SubscriberPollOnPollInHandler(Socket socket, IOMultiPlex revents)
        {
            Console.WriteLine("Received weather");
            socket.Recv();
        }

        private static void ReceiverPollOnPollInHandler(Socket socket, IOMultiPlex revents)
        {
            Console.WriteLine("Received ventilator");
            socket.Recv();
        }
    }
}