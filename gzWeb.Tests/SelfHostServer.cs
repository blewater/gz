using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Security.DataProtection;

namespace gzWeb.Tests
{
    public class SelfHostServer : IDisposable
    {
        /// <summary>
        /// 
        /// Get first free port. Make self hosting easier to deploy.
        /// 
        /// </summary>
        /// <returns></returns>
        private static int FreeTcpPort() {

            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();

            return port;
        }

        public static SelfHostServer Start(string uriStr)
        {
            Port = FreeTcpPort();

            return new SelfHostServer(uriStr);
        }

        public HttpClient Client
        {
            get { return new HttpClient {BaseAddress = _uri}; }
        }

        private SelfHostServer(string uriStr) {

            var fullUrl = uriStr + ':' + 8096;

            _uri = new Uri(fullUrl);

            _disposable = WebApp.Start<gzWeb.Startup>(_uri.AbsoluteUri);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }

        private readonly Uri _uri;
        private readonly IDisposable _disposable;

        public static int Port { get; private set; }
    }
}