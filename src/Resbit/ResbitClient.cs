using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Web;
using JsonFx.Json;

namespace Resbit
{
    public class ResbitClient
    {
        private const string DefaultVHost = "%2F";
        private readonly Func<WebClient> webClient;
        private readonly JsonReader jsonReader = new JsonReader();
        readonly JsonWriter jsonWriter = new JsonWriter();
        private readonly string baseAddress;
        private readonly NetworkCredential networkCredential;

        public ResbitClient(string hostName, string username = "guest", string password = "guest")
        {
            baseAddress = "http://" + hostName + ":55672/api/";
            networkCredential = new NetworkCredential(username, password);

            webClient = () =>
                        new WebClient
                        {
                            BaseAddress = baseAddress,
                            Credentials = networkCredential,
                        };
        }

        public dynamic AllConfiguration()
        {
            return Get("all-configuration");
        }

        private dynamic Get(string resource)
        {
            using (var client = webClient())
                return jsonReader.Read(client.DownloadString(resource));
        }

        public dynamic Overview()
        {
            return Get("overview");
        }

        public dynamic[] Nodes()
        {
            return Get("nodes");
        }

        public dynamic[] Connections()
        {
            return Get("connections");
        }

        public dynamic GetConnection(string name)
        {
            return Get(string.Format("connections/{0}", name));
        }

        public void DeleteConnection(string name)
        {
            Delete(string.Format("connections/{0}", name));            
        }

        private void Delete(string resource)
        {
            var uri = new Uri(new Uri(baseAddress), resource);
            ForceCanonicalPathAndQuery(uri);

            var request = WebRequest.Create(uri);
            request.Credentials = networkCredential;
            request.Method = "DELETE";
            using(request.GetResponse());
        }

        private void Put(Uri resource, object payload)
        {
            var request = WebRequest.Create(resource);
            request.ContentType = "application/json";
            request.Credentials = networkCredential;
            request.Method = "PUT";

            using (var w = new StreamWriter(request.GetRequestStream()))
                jsonWriter.Write(payload, w);

            using (request.GetResponse()) ;
        }

        public dynamic[] Channels()
        {
            return Get("channels");
        }

        public dynamic Channel(string name)
        {
            return Get(string.Format("channels/{0}", name));
        }

        public dynamic[] Exchanges()
        {
            return Get("exchanges");
        }

        public dynamic Exchanges(string vhost)
        {
            return Get(string.Format("exchanges/{0}", vhost));
        }

        public dynamic GetExchange(string name)
        {
            return GetExchange(DefaultVHost, name);
        }

        public dynamic GetExchange(string vhost, string name)
        {
            var resource = new Uri(new Uri(baseAddress), string.Format("exchanges/{0}/{1}", vhost, name));

            ForceCanonicalPathAndQuery(resource);

            return Get(resource);
        }

        public void PutExchange(string toString)
        {
            throw new NotImplementedException("TODO");
        }

        public dynamic[] Queues()
        {
            return Get("queues");
        }

        public dynamic[] Queues(string vhost)
        {
            return Get(string.Format("queues/{0}", vhost));
        }

        public dynamic GetQueue(string name)
        {
            return GetQueue(DefaultVHost, name);
        }

        public dynamic GetQueue(string vhost, string name)
        {
            var resource = new Uri(new Uri(baseAddress), string.Format("queues/{0}/{1}", vhost, HttpUtility.UrlEncode(name)));

            ForceCanonicalPathAndQuery(resource);

            return Get(resource);
        }

        public void PutQueue(string name)
        {
            PutQueue(DefaultVHost, name);
        }

        public void PutQueue(string vhost, string name, object options = null)
        {
            var resource = new Uri(new Uri(baseAddress), string.Format("queues/{0}/{1}", vhost, name));

            ForceCanonicalPathAndQuery(resource);

            Put(resource, options ?? new object());
        }

        public void DeleteQueue(string name)
        {
            DeleteQueue(DefaultVHost, name);
        }

        private void DeleteQueue(string vhost, string name)
        {
            Delete(string.Format("queues/{0}/{1}", vhost, name));            
        }

        private dynamic Get(Uri resource)
        {
            using (var client = webClient())
                return jsonReader.Read(client.DownloadString(resource)); 
        }

        /// <summary>
        /// http://stackoverflow.com/questions/781205/c-net-getting-a-url-with-an-url-encoded-slash
        /// </summary>
        static void ForceCanonicalPathAndQuery(Uri uri)
        {
            string paq = uri.PathAndQuery; // need to access PathAndQuery
            FieldInfo flagsFieldInfo = typeof(Uri).GetField("m_Flags", BindingFlags.Instance | BindingFlags.NonPublic);
            ulong flags = (ulong)flagsFieldInfo.GetValue(uri);
            flags &= ~((ulong)0x30); // Flags.PathNotCanonical|Flags.QueryNotCanonical
            flagsFieldInfo.SetValue(uri, flags);
            paq = string.Empty;
        }
    }
}