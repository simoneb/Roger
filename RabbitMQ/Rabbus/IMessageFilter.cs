using System.Collections.Generic;

namespace Rabbus
{
    public interface IMessageFilter
    {
        IEnumerable<CurrentMessageInformation> Filter(IEnumerable<CurrentMessageInformation> input);
    }
}