using System.Text;
using ZeroMQExtensions;
using ZMQ;

namespace ZeroMQ_Guide.Guide
{
    public class Identity : Runnable
    {
        public override void Run()
        {
            using (var context = new Context(1))
            using (var sink = context.Router().Bind("inproc://example"))
            using (var anonymous = context.Req().Connect("inproc://example"))
            using (var identified = context.Req("Hello").Connect("inproc://example"))
            {
                anonymous.Send("ROUTER uses generated UUID", Encoding.UTF8);

                sink.Dump();

                identified.Send("ROUTER uses REQ's identity", Encoding.UTF8);

                sink.Dump();
            }
        }
    }
}