using System;
using System.Text;
using ZMQ;
using System.Linq;

namespace ZeroMQ_Guide.Guide
{
    public class Weather : Runnable
    {
        public override void Run()
        {
            Run(Publisher);
            Run(Subscriber);
        }

        private static void Subscriber()
        {
            using (var context = new Context(1))
            using (var subscriber = context.Socket(SocketType.SUB))
            {
                subscriber.Subscribe("10001 ", Encoding.UTF8);
                subscriber.Connect("tcp://localhost:5556");

                int totalTemp = 0;

                int updateNo;
                for (updateNo = 0; updateNo < 100; updateNo++)
                {
                    var message = subscriber.Recv(Encoding.UTF8);

                    var values = message.Split(' ').Select(int.Parse);

                    totalTemp += values.ElementAt(1);
                }

                Console.WriteLine("Average temp for zipcode was {0}", totalTemp/updateNo);
            }
        }

        private static void Publisher()
        {
            using (var context = new Context(1))
            using (var publisher = context.Socket(SocketType.PUB))
            {
                publisher.Bind("tcp://*:5556");
                //publisher.Bind("ipc://weather.ipc");

                var rnd = new Random();

                while (true)
                {
                    var zip = rnd.Next(9990, 10010);
                    var temp = rnd.Next(215) - 80;
                    var relHumidity = rnd.Next(50) + 10;

                    publisher.Send(string.Format("{0} {1} {2}", zip, temp, relHumidity), Encoding.UTF8);
                }
            }
        }
    }
}