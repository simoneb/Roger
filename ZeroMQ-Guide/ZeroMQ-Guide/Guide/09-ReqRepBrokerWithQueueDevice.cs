using ZMQ;

namespace ZeroMQ_Guide.Guide
{
    public class ReqRepBrokerWithQueueDevice : ReqRepBroker
    {
        protected override void Broker()
        {
            using (var context = new Context(1))
            using (var inSocket = context.Socket(SocketType.XREP))
            using (var outSocket = context.Socket(SocketType.XREQ))
            {
                inSocket.Bind("tcp://*:5559");
                outSocket.Bind("tcp://*:5560");

                Socket.Device.Queue(inSocket, outSocket);
            }
        }
    }
}