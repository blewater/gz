using System;
using System.Data;
using System.Data.Entity;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using System.Web.Mvc;
using gzDAL.Conf;
using gzDAL.Models;
using gzWeb.Models;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using NUnit.Framework;

namespace gzWeb.Tests.Controllers
{
    public abstract class BaseApiControllerTests
    {
        protected SelfHostServer Server { get; private set; }
        protected HttpClient Client { get { return Server.Client; } }
        //protected TransactionScope TransactionScope { get; private set; }

        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
            Server = SelfHostServer.Start(new Uri("http://localhost:9090"));
            //TransactionScope=new TransactionScope();
        }

        [OneTimeTearDown]
        public virtual void OneTimeTearDown()
        {
            //TransactionScope.Dispose();
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
        //private DbContextTransaction _dbTransaction;
        private TransactionScope TransactionScope;

        [SetUp]
        public void SetUp()
        {
            //TransactionScope = new TransactionScope();
            //var dbContext = DependencyResolver.Current.GetService<ApplicationDbContext>();
            //_dbTransaction = dbContext.Database.BeginTransaction();
        }

        [TearDown]
        public void TearDown()
        {
            //TransactionScope.Dispose();
            //_dbTransaction.Rollback();
        }

        [Test]
        public async Task ShouldFailWithInvalidModel()
        {
            var response = await Client.PostJsonAsync("/api/Account/Register", new RegisterBindingModel {Username = "username"});

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task ShouldNotFailWithValidModel()
        {
            //var response = await TestServer.Create<gzWeb.Startup>()
            //                         .HttpClient.PostJsonAsync("/api/Account/Register", new RegisterBindingModel
            //                         {
            //                             Username = "username",
            //                             Email = "email@email.com",
            //                             Password = "1234567",
            //                             FirstName = "FirstName",
            //                             LastName = "LastName",
            //                             Birthday = new DateTime(1975, 10, 13),
            //                             Currency = "EUR",
            //                         });

            //var response = await Client.PostJsonAsync("/api/Account/Register", new RegisterBindingModel
            //{
            //    Username = "username",
            //    Email = "email@email.com",
            //    Password = "1234567",
            //    FirstName = "FirstName",
            //    LastName = "LastName",
            //    Birthday = new DateTime(1975, 10, 13),
            //    Currency = "EUR",
            //});

            //Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }
}