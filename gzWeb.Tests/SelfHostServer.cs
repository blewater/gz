using System;
using System.Net.Http;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Security.DataProtection;

namespace gzWeb.Tests
{
    public class SelfHostServer : IDisposable
    {
        public static SelfHostServer Start(Uri uri)
        {
            return new SelfHostServer(uri);
        }

        public HttpClient Client
        {
            get { return new HttpClient {BaseAddress = _uri}; }
        }

        private SelfHostServer(Uri uri)
        {
            _uri = uri;
            var startOptions = new StartOptions();
            startOptions.Urls.Add(_uri.AbsoluteUri);
            startOptions.Settings.Add("UnitTest.security.DataProtectionProvider","");
            _disposable = WebApp.Start<gzWeb.Startup>(startOptions);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }

        private readonly Uri _uri;
        private readonly IDisposable _disposable;
    }
}