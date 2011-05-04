using System.Threading;
using ZMQ;
using ZeroMQExtensions;

namespace ZeroMQ_Guide.Guide
{
    public class WeatherProxy : Runnable
    {
        public override void Run()
        {
            var handle = new ManualResetEvent(false);
            Run(Weather.Publisher, handle);

            Run(Proxy);
            Run(Weather.Subscriber, "tcp://localhost:5556");
            Run(Weather.Subscriber, "tcp://localhost:55567");

            Thread.Sleep(1000);

            handle.Set();
        }

        private static void Proxy()
        {
            using (var context = new Context(1))
            using (var frontend = context.Sub().SubscribeAll().Connect("tcp://localhost:5556"))
            using (var backend = context.Pub().Bind("tcp://*:55567"))
            {
                Thread.Sleep(1000);

                while (true)
                    backend.Send(frontend.Recv());
            }
        }
    }
}