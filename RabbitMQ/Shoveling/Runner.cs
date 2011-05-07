using System.Collections.Generic;
using Common;

namespace Shoveling
{
    public class Runner : IProcessesProvider
    {
        public IEnumerable<IProcess> Processes
        {
            get
            {
                yield return new PublisherOnMain();
                yield return new PublisherOnSecondary();
                yield return new SubscriberOnMain();
                yield return new SubscriberOnSecondary();
            }
        }
    }
}
