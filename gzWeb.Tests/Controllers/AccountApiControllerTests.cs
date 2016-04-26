using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using gzWeb.Models;
using Newtonsoft.Json;
using NUnit.Framework;

namespace gzWeb.Tests.Controllers
{
    public abstract class BaseApiControllerTests
    {
        protected static SelfHostServer Server { get; private set; }
        protected static HttpClient Client { get { return Server.Client; } }

        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
            Server = SelfHostServer.Start(new Uri("http://localhost:9090"));
        }

        [OneTimeTearDown]
        public virtual void OneTimeTearDown()
        {
            Server.Dispose();
        }
    }

    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PostJsonAsync<TPayload>(this HttpClient @this, string relativeUri, TPayload payload)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, relativeUri);
            var json = JsonConvert.SerializeObject(payload);
            request.Content = new StringContent(json);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return await @this.SendAsync(request);
        }
    }

    [TestFixture]
    public class AccountApiControllerTests : BaseApiControllerTests
    {
        [SetUp]
        public void SetUp()
        {

        }

        [TearDown]
        public void TearDown()
        {

        }

        [Test]
        public async Task ShouldFailWithInvalidModel()
        {
            var response = await Client.PostJsonAsync("/api/Account/Register", new RegisterBindingModel {Username = "username"});

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}