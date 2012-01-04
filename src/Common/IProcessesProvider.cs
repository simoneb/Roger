using System.Collections.Generic;

namespace Common
{
    public interface IProcessesProvider
    {
        IEnumerable<IProcess> Processes { get; }
    }
}