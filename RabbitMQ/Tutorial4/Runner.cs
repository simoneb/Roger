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
                yield return new Consumer(Severities.Debug, Severities.Info);
                yield return new Consumer(Severities.Warning, Severities.Error);
                yield return new Consumer(Severities.Debug, Severities.Error);
            }
        }
    }
}