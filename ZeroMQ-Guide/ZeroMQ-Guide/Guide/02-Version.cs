using System;
using ZMQ;

namespace ZeroMQ_Guide.Guide
{
    public class Version : Runnable
    {
        public override void Run()
        {
            Console.WriteLine(ZHelpers.Version());
        }
    }
}