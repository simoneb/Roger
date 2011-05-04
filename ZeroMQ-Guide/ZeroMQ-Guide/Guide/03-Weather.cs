using System;
using System.Text;
using System.Threading;
using ZMQ;
using System.Linq;
using ZeroMQExtensions;

namespace ZeroMQ_Guide.Guide
{
    public class Weather : Runnable
    {
        public override void Run()
        {
            Run(Publisher);
            Run(Subscriber, "tcp://localhost:5556");
        }

        public static void Publisher()
        {
            Publisher(new ManualResetEvent(true));
        }

        public static void Publisher(WaitHandle handle)
        {
            using (var context = new Context(1))
            using (var publisher = context.Pub().Bind("tcp://*:5556"))
            {
                handle.WaitOne();

                var rnd = new Random();

                while (true)
                {
                    var zip = rnd.Next(9500, 10500);
                    var temp = rnd.Next(215) - 80;
                    var relHumidity = rnd.Next(50) + 10;

                    publisher.Send(string.Format("{0} {1} {2}", zip, temp, relHumidity));
                }
            }
        }

        public static void Subscriber(string address)
        {
            using (var context = new Context(1))
            using (var subscriber = context.Sub().Subscribe("10001 ").Connect(address))
            {
                int totalTemp = 0;

                int updateNo;
                for (updateNo = 0; updateNo < 100; updateNo++)
                {
                    var message = subscriber.Recv(Encoding.UTF8);

                    var values = message.Split(' ').Select(int.Parse);

                    totalTemp += values.ElementAt(1);
                }

                Console.WriteLine("Average temp for zipcode was {0}", (double)totalTemp/updateNo);
            }
        }
    }
}