using System.Collections.Generic;
using Common;

namespace Tutorial3
{
    public class Runner : IProcessesProvider
    {
        public IEnumerable<IProcess> Processes
        {
            get
            {
                yield return new Publisher();
                yield return new Consumer();
                yield return new Consumer();
            }
        }
    }
}