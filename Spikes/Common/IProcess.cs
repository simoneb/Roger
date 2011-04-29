using System.Threading;

namespace Common
{
    public interface IProcess
    {
        void Start(WaitHandle waitHandle);
    }
}