using System.Collections.Generic;
using Common;

namespace Tutorial1
{
    public class Runner : IProcessesProvider
    {
        public IEnumerable<IProcess> Processes
        {
            get
            {
                yield return new Publisher();
                yield return new Receiver();
            }
        }
    }
}