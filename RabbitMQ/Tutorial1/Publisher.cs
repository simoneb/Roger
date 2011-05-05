using System;
using System.Text;
using System.Threading;
using Common;

namespace Tutorial1
{
    [Serializable]
    public class Publisher : IProcess
    {
        public void Start(WaitHandle waitHandle)
        {
            using (var connection = Helpers.CreateConnection())
            using (var model = connection.CreateModel())
            {
                model.QueueDeclare(Constants.QueueName, false, false, false, null);

                model.BasicPublish("", Constants.QueueName, null, Encoding.UTF8.GetBytes("ciao!"));

                Console.WriteLine("Published!");
            }
        }
    }
}
