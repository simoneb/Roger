using System.Threading;
using System.Threading.Tasks;

namespace Tests.Unit
{
    public static class TaskExtensions
    {
         public static void WaitForStart(this Task task)
         {
             SpinWait.SpinUntil(task.StartedOrCompleted, 1000);
         }

        public static bool StartedOrCompleted(this Task task)
        {
            return 
                task.Status == TaskStatus.Canceled ||
                task.Status == TaskStatus.Faulted ||
                task.Status == TaskStatus.RanToCompletion ||
                task.Status == TaskStatus.Running;
        }
    }
}