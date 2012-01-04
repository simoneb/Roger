using System.Collections.Generic;
using Common;

namespace Tutorial4
{
    public class Runner : IProcessesProvider
    {
        public IEnumerable<IProcess> Processes
        {
            get
            {
                yield return new Publisher();
                yield return new Consumer(Severity.Debug, Severity.Info);
                yield return new Consumer(Severity.Warning, Severity.Error);
                yield return new Consumer(Severity.Debug, Severity.Error);
            }
        }
    }
}