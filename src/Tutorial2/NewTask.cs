using System;
using System.Text;
using System.Threading;
using Common;

namespace Tutorial2
{
    [Serializable]
    public class NewTask : IProcess
    {
        public void Start(WaitHandle waitHandle)
        {
            using (var connection = Helpers.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(Constants.QueueName, true, false, false, null);

                var basicProperties = channel.CreateBasicProperties();
                basicProperties.DeliveryMode = 2;

                while (!waitHandle.WaitOne(TimeSpan.FromSeconds(5)))
                {
                    channel.BasicPublish("",
                                         Constants.QueueName,
                                         basicProperties,
                                         Encoding.UTF8.GetBytes("Ciao " + DateTime.Now.ToLongTimeString()));
                    Console.WriteLine("Message published");
                }
            }
        }
    }
}