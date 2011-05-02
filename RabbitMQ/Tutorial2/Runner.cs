using System.Collections.Generic;
using Common;
using RabbitMQ.Client.Impl;

namespace Tutorial2
{
    public class Runner : IProcessesProvider
    {
        public IEnumerable<IProcess> Processes
        {
            get
            {
                yield return new NewTask();
                yield return new Worker();
                yield return new Worker();
            }
        }
    }
}