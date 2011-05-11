using System;
using System.Collections.Generic;
using System.Threading;
using Common;
using Resbit;

namespace Temporary
{
    [Serializable]
    public class AlivenessTest : IProcessesProvider, IProcess
    {
        public IEnumerable<IProcess> Processes { get { yield return this; } }

        public void Start(WaitHandle waitHandle)
        {
            var client = new ResbitClient("http://localhost:55672", "guest", "guest");

            Console.WriteLine(client.AlivenessTest.Get(""));

            waitHandle.WaitOne();
        }
    }
}