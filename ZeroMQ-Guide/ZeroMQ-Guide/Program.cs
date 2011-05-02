using System;
using System.Text;
using System.Threading;
using ZMQ;

namespace ZeroMQ_Guide
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var context = new Context())
            using (Socket subscriber = context.Socket(SocketType.SUB), publisher = context.Socket(SocketType.PUB))
            {
                publisher.Bind("tcp://127.0.0.1:5556");

                Thread.Sleep(200);

                subscriber.Connect("tcp://127.0.0.1:5556");
                subscriber.Subscribe("NASDAQ", Encoding.UTF8);

                while (true)
                {
                    publisher.SendMore("MILANO", Encoding.UTF8);
                    publisher.Send("I will not receive this", Encoding.UTF8);

                    publisher.SendMore("NASDAQ", Encoding.UTF8);
                    publisher.Send("I will receive this", Encoding.UTF8);

                    foreach (var msg in subscriber.RecvAll(Encoding.UTF8))
                        Console.WriteLine(msg);

                    Thread.Sleep(1000);
                }
            }
        }
    }
}
