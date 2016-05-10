using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using System.Web.Mvc;
using gzDAL.Models;
using gzWeb.Areas.Mvc.Models;
using gzWeb.Models;
using Newtonsoft.Json;
using NUnit.Framework;

namespace gzWeb.Tests.Controllers
{
    public class DatabaseInitializer : DropCreateDatabaseAlways<TestDbContext> {
        protected override void Seed(TestDbContext context) {

            gzDAL.Conf.TestSeeder.GenData();
        }
    }

    public class LoginResult
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string userName { get; set; }
    }

    public abstract class BaseApiControllerTests {

        protected SelfHostServer Server { get; private set; }
        protected HttpClient Client { get { return Server.Client; } }

        protected const string UnitTestDb = "gzTestDb";

        [OneTimeSetUp]
        public virtual void OneTimeSetUp() {

            Server = SelfHostServer.Start("http://localhost");

            // Uncomment following line and comment next 2 if wanting to test with TestDb
            // Replace also ApplicationDbContext with TestDbContext
            //Database.SetInitializer<TestDbContext>(new DatabaseInitializer());

            Database.SetInitializer<ApplicationDbContext>(null);
            gzDAL.Conf.Seed.GenData();

        }

        [OneTimeTearDown]
        public virtual void OneTimeTearDown()
        {
            Server.Dispose();
        }
    }

    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PostJsonAsync<TValue>(this HttpClient @this, string relativeUri, TValue value)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, relativeUri);
            var json = JsonConvert.SerializeObject(value);
            request.Content = new StringContent(json);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return await @this.SendAsync(request);
        }

        public static async Task<HttpResponseMessage> PostJsonAuthAsync<TValue>(this HttpClient @this, string relativeUri, string token, TValue value)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, relativeUri);
            var json = JsonConvert.SerializeObject(value);
            request.Headers.Add("authorization", String.Format("Bearer {0}", token));
            request.Content = new StringContent(json);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            
            return await @this.SendAsync(request);
        }

        //public static async Task<HttpResponseMessage> PostAsync<TPayload>(this HttpClient @this, string relativeUri, TPayload payload)
        //{
        //    var request = new HttpRequestMessage(HttpMethod.Post, relativeUri);
        //    var json = JsonConvert.SerializeObject(payload);
        //    request.Content = new StringContent(json);
        //    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        //    return await @this.SendAsync(request);
        //}
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

        #region Account/Login

        [Test]
        [Category("Account/Login")]
        public async Task LoginShouldPass() {

            var response = await Client.PostAsync("/Token",
                                                  new FormUrlEncodedContent(new[]
                                                                            {
                                                                                    new KeyValuePair<string, string>("grant_type", "password"),
                                                                                    new KeyValuePair<string, string>("username", "testuser"),
                                                                                    new KeyValuePair<string, string>("password", "gz2016!@"),
                                                                            }));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        [Category("Account/Login")]
        public async Task LoginShouldFailWithWrongUser()
        {
            var response = await Client.PostAsync("/Token",
                                                  new FormUrlEncodedContent(new[]
                                                                            {
                                                                                    new KeyValuePair<string, string>("grant_type", "password"),
                                                                                    new KeyValuePair<string, string>("username", "unknown"),
                                                                                    new KeyValuePair<string, string>("password", "gz2016!@"),
                                                                            }));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        [Category("Account/Login")]
        public async Task LoginShouldFailWithWrongPassword()
        {
            var response = await Client.PostAsync("/Token",
                                                  new FormUrlEncodedContent(new[]
                                                                            {
                                                                                    new KeyValuePair<string, string>("grant_type", "password"),
                                                                                    new KeyValuePair<string, string>("username", "testuser"),
                                                                                    new KeyValuePair<string, string>("password", "wrongpassword"),
                                                                            }));
            
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        [Test]
        public async Task ForgotPasswordShouldGeneratePasswordResetToken()
        {
            await Client.PostJsonAsync("/api/Account/Register",
                                        new RegisterBindingModel
                                        {
                                                Username = "username",
                                                Email = "email@email.com",
                                                Password = "1234567",
                                                FirstName = "FirstName",
                                                LastName = "LastName",
                                                Birthday = new DateTime(1975, 10, 13),
                                                Currency = "EUR",
                                        });

            using (var dbContext = new ApplicationDbContext())
            {
                var user = dbContext.Users.Single(x => x.UserName == "username");
                Assert.NotNull(user);

                var response = await Client.PostJsonAsync("/api/Account/ForgotPassword", new ForgotPasswordViewModel { Email = user.Email });
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                var code = await response.Content.ReadAsStringAsync();
                Assert.IsTrue(!String.IsNullOrEmpty(code));

                dbContext.Users.Remove(user);
                dbContext.SaveChanges();
            }
        }

        [Test]
        public async Task ForgotPasswordShouldFailIfUserNotExists()
        {
            var response = await Client.PostJsonAsync("/api/Account/ForgotPassword", new ForgotPasswordViewModel { Email = "unknown@where.com" });
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task ResetPasswordShouldResetPassword()
        {
            await Client.PostJsonAsync("/api/Account/Register",
                                        new RegisterBindingModel
                                        {
                                            Username = "username",
                                            Email = "email@email.com",
                                            Password = "1234567",
                                            FirstName = "FirstName",
                                            LastName = "LastName",
                                            Birthday = new DateTime(1975, 10, 13),
                                            Currency = "EUR",
                                        });
            using (var dbContext = new ApplicationDbContext())
            {
                var user = dbContext.Users.Single(x => x.UserName == "username");
                Assert.NotNull(user);

                var forgotPasswordResponse = await Client.PostJsonAsync("/api/Account/ForgotPassword", new ForgotPasswordViewModel { Email = user.Email });
                Assert.AreEqual(HttpStatusCode.OK, forgotPasswordResponse.StatusCode);

                var code = await forgotPasswordResponse.Content.ReadAsStringAsync();
                Assert.IsTrue(!String.IsNullOrEmpty(code));
                code = code.Trim('"');

                var resetPasswordResponse = await Client.PostJsonAsync("/api/Account/ResetPassword", new ResetPasswordViewModel
                                                                                        {
                                                                                                Code = code,
                                                                                                Email = user.Email,
                                                                                                Password = "7654321",
                                                                                                ConfirmPassword = "7654321",
                                                                                        });
                Assert.AreEqual(HttpStatusCode.OK, resetPasswordResponse.StatusCode);

                var loginResponse = await Client.PostAsync("/Token",
                                                  new FormUrlEncodedContent(new[]
                                                                            {
                                                                                    new KeyValuePair<string, string>("grant_type", "password"),
                                                                                    new KeyValuePair<string, string>("username", user.Email),
                                                                                    new KeyValuePair<string, string>("password", "7654321"),
                                                                            }));

                Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode);

                var failedLoginResponse = await Client.PostAsync("/Token",
                                                  new FormUrlEncodedContent(new[]
                                                                            {
                                                                                    new KeyValuePair<string, string>("grant_type", "password"),
                                                                                    new KeyValuePair<string, string>("username", user.Email),
                                                                                    new KeyValuePair<string, string>("password", "1234567"),
                                                                            }));

                Assert.AreEqual(HttpStatusCode.BadRequest, failedLoginResponse.StatusCode);

                dbContext.Users.Remove(user);
                dbContext.SaveChanges();
            }
        }

        [Test]
        public async Task ChangePasswordShouldChangePassword()
        {
            await Client.PostJsonAsync("/api/Account/Register",
                                        new RegisterBindingModel
                                        {
                                            Username = "username",
                                            Email = "email@email.com",
                                            Password = "1234567",
                                            FirstName = "FirstName",
                                            LastName = "LastName",
                                            Birthday = new DateTime(1975, 10, 13),
                                            Currency = "EUR",
                                        });

            using (var dbContext = new ApplicationDbContext())
            {
                var user = dbContext.Users.Single(x => x.UserName == "username");
                Assert.NotNull(user);

                var firstLoginResponse = await Client.PostAsync("/Token",
                                                  new FormUrlEncodedContent(new[]
                                                                            {
                                                                                    new KeyValuePair<string, string>("grant_type", "password"),
                                                                                    new KeyValuePair<string, string>("username", user.Email),
                                                                                    new KeyValuePair<string, string>("password", "1234567"),
                                                                            }));
                Assert.AreEqual(HttpStatusCode.OK, firstLoginResponse.StatusCode);
                var loginResult = JsonConvert.DeserializeObject<LoginResult>(await firstLoginResponse.Content.ReadAsStringAsync());

                var changePasswordResponse = await Client.PostJsonAuthAsync("/api/Account/ChangePassword",
                                                                            loginResult.access_token,
                                                                            new ChangePasswordBindingModel
                                                                            {
                                                                                    OldPassword = "1234567",
                                                                                    NewPassword = "7654321",
                                                                                    ConfirmPassword = "7654321"
                                                                            });
                Assert.AreEqual(HttpStatusCode.OK, changePasswordResponse.StatusCode);


                var loginResponse = await Client.PostAsync("/Token",
                                                  new FormUrlEncodedContent(new[]
                                                                            {
                                                                                    new KeyValuePair<string, string>("grant_type", "password"),
                                                                                    new KeyValuePair<string, string>("username", user.Email),
                                                                                    new KeyValuePair<string, string>("password", "7654321"),
                                                                            }));

                Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode);

                var failedLoginResponse = await Client.PostAsync("/Token",
                                                  new FormUrlEncodedContent(new[]
                                                                            {
                                                                                    new KeyValuePair<string, string>("grant_type", "password"),
                                                                                    new KeyValuePair<string, string>("username", user.Email),
                                                                                    new KeyValuePair<string, string>("password", "1234567"),
                                                                            }));

                Assert.AreEqual(HttpStatusCode.BadRequest, failedLoginResponse.StatusCode);

                dbContext.Users.Remove(user);
                dbContext.SaveChanges();
            }
        }
        [Test]
        public async Task ShouldFailWithInvalidModel()
        {
            var response = await Client.PostJsonAsync("/api/Account/Register", new RegisterBindingModel {Username = "username"});

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            using (var dbContext = new ApplicationDbContext())
                Assert.IsFalse(dbContext.Users.Any(x => x.UserName == "username"));
        }

        [Test]
        public async Task ShouldNotFailWithValidModel() {

            var db = new ApplicationDbContext();
            var user = db.Users.SingleOrDefault(u => u.Email == "email@email.com");
            if (user != null) {
                db.Users.Remove(user);
                db.SaveChanges();
            }

            var response = await Client.PostJsonAsync("/api/Account/Register",
                                                      new RegisterBindingModel
                                                      {
                                                              Username = "username",
                                                              Email = "email@email.com",
                                                              Password = "1234567",
                                                              FirstName = "FirstName",
                                                              LastName = "LastName",
                                                              Birthday = new DateTime(1975, 10, 13),
                                                              Currency = "EUR",
                                                      });

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            using (var dbContext = new ApplicationDbContext())
            {
                user = dbContext.Users.Single(x => x.UserName == "username");
                Assert.NotNull(user);

                dbContext.Users.Remove(user);
                dbContext.SaveChanges();
            }
        }
    }
}