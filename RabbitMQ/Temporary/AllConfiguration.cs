using System;
using System.Collections.Generic;
using System.Threading;
using Common;
using Hammock;
using Hammock.Authentication.Basic;
using Hammock.Web;

namespace Temporary
{
    [Serializable]
    public class AllConfiguration : IProcessesProvider, IProcess
    {
        public IEnumerable<IProcess> Processes { get { yield return this; } }

        public void Start(WaitHandle waitHandle)
        {
            var client = new RestClient
            {
                Credentials = new BasicAuthCredentials {Username = "guest", Password = "guest"},
                Authority = "http://localhost:55672/api"
            };

            var response = client.Request(new RestRequest
            {
                Path = "all-configuration",
                Method = WebMethod.Get,
            });

            Console.WriteLine(response.Content);

            waitHandle.WaitOne();
        }
    }
}
