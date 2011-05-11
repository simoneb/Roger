using System;
using System.Collections.Generic;
using System.Threading;
using Common;
using Resbit;

namespace Temporary
{
    [Serializable]
    public class WhoAmI : IProcessesProvider, IProcess
    {
        public IEnumerable<IProcess> Processes { get { yield return this; } }

        public void Start(WaitHandle waitHandle)
        {
            var client = new ResbitClient("http://localhost:55672", "guest", "guest");

            foreach (var property in (IDictionary<string, object>)client.WhoAmI.Get())
                Console.WriteLine(property.Key);

            waitHandle.WaitOne();
        }
    }
}