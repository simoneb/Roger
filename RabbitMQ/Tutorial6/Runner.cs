using System.Collections.Generic;
using Common;

namespace Tutorial6
{
    public class Runner : IProcessesProvider
    {
        public IEnumerable<IProcess> Processes
        {
            get
            {
                yield return new FibonacciService();
                yield return new FibonacciClient(30);
            }
        }
    }
}
