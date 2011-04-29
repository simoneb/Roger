using System.Collections.Generic;
using Common;

namespace Tutorial5
{
    public class Runner : IProcessesProvider
    {
        public IEnumerable<IProcess> Processes
        {
            get
            {
                yield return new Publisher();
                yield return new Consumer("1.#");
                yield return new Consumer("1.*.2");
                yield return new Consumer("3.*.*");
            }
        }
    }
}
