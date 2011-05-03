using System.Text;
using System.Threading;
using ZMQ;

namespace ZeroMQ_Guide.Guide
{
    public class WeatherProxy : Runnable
    {
        public override void Run()
        {
            Run(Weather.Subscriber, "tcp://localhost:5556");
            Run(Weather.Subscriber, "tcp://localhost:55567");

            Thread.Sleep(100);

            Run(Proxy);

            Thread.Sleep(1000);

            Run(Weather.Publisher);
        }

        private static void Proxy()
        {
            using (var context = new Context(1))
            using (var frontend = context.Socket(SocketType.SUB))
            using (var backend = context.Socket(SocketType.PUB))
            {
                frontend.Connect("tcp://localhost:5556");
                frontend.Subscribe("", Encoding.UTF8);

                backend.Bind("tcp://*:55567");

                while (true)
                {
                    foreach (var message in frontend.RecvAll())
                        backend.Send(message);
                }
            }
        }
    }
}