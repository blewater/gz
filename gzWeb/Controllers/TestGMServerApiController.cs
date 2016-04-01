using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using gzWeb.Areas.Mvc.Models;
using Newtonsoft.Json;
using NLog.LayoutRenderers;
using RestSharp;

namespace gzWeb.Controllers
{
    public class BoolToInt32Converter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((bool)value) ? 1 : 0);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value.ToString() == "1";
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(bool);
        }
    }

    public class Error
    {
        [Newtonsoft.Json.JsonProperty("errorCode")]
        public int Code { get; set; }

        [Newtonsoft.Json.JsonProperty("errorMessage")]
        public string Message { get; set; }

        [Newtonsoft.Json.JsonProperty("errorDetails")]
        public string[] Details { get; set; }

        [Newtonsoft.Json.JsonProperty("logId")]
        public long LogId { get; set; }
    }

    public class EveryMatrixResponseBase
    {
        [JsonProperty("errorData")]
        public Error Error { get; set; }

        [JsonProperty("isAvailable")]
        [JsonConverter(typeof(BoolToInt32Converter))]
        public bool IsAvailable { get; set; }

        [JsonProperty("success")]
        [JsonConverter(typeof(BoolToInt32Converter))]
        public bool Success { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }

    public class UserResponseBase : EveryMatrixResponseBase
    {
        [JsonProperty("userId")]
        public long UserId { get; set; }
    }

    public enum ActiveStatus
    {
        InActive = -1,
        Active = 0,
        Blocked = 1,
        Closed = 2,
    }

    public class CheckUserExistsResponse : EveryMatrixResponseBase
    {
        [JsonProperty("activeStatus")]
        public ActiveStatus ActiveStatus { get; set; }

        [JsonProperty("userId")]
        public long UserId { get; set; }
    }

    public class UserLoginResponse : UserResponseBase
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("sessionId")]
        public string SessionId { get; set; }
    }

    public class GamMatrixClient
    {
        public Task<bool> IsUserNameAvailable(string username)
        {
            var client = new RestClient(ServiceUrl);
            var request = new RestRequest(String.Format("{0}/{1}", BuildUrl("IsUserNameAvailable"), username), Method.GET);
            
            return client.ExecuteTaskAsync(request)
                         .ContinueWith(task => JsonConvert.DeserializeObject<EveryMatrixResponseBase>(task.Result.Content).IsAvailable);
        }

        public Task<bool> IsEmailAvailable(string email)
        {
            var client = new RestClient(ServiceUrl);
            var request = new RestRequest(String.Format("{0}/{1}", BuildUrl("IsEmailAvailable"), email), Method.GET);

            return client.ExecuteTaskAsync(request)
                         .ContinueWith(task => JsonConvert.DeserializeObject<EveryMatrixResponseBase>(task.Result.Content).IsAvailable);
        }

        public Task<bool> IsAliasAvailable(string alias)
        {
            var client = new RestClient(ServiceUrl);
            var request = new RestRequest(String.Format("{0}/{1}", BuildUrl("IsAliasAvailable"), alias), Method.GET);

            return client.ExecuteTaskAsync(request)
                         .ContinueWith(task => JsonConvert.DeserializeObject<EveryMatrixResponseBase>(task.Result.Content).IsAvailable);
        }

        public Task<long> CheckUserExists(DateTime birthDate, string email)
        {
            var client = new RestClient(ServiceUrl);
            var request = new RestRequest(BuildUrl("CheckUserExists"), Method.POST);
            request.AddJsonBody(new
                                {
                                        birthDate = birthDate.ToString("yyyy-MM-dd"),
                                        email = email
                                });


            return client.ExecuteTaskAsync(request)
                         .ContinueWith(task => JsonConvert.DeserializeObject<CheckUserExistsResponse>(task.Result.Content).UserId);
        }

        public Task<object> RegisterUser(RegisterUserViewModel model, string ipAddress)
        {
            var client = new RestClient(ServiceUrl);
            var request = new RestRequest(BuildUrl("RegisterUser"), Method.POST);
            request.AddJsonBody(new
                                {
                                        //activateAccount = "1",
                                        userName = model.Username,
                                        password = model.Password,
                                        title = model.Title,
                                        firstName = model.FirstName,
                                        lastName = model.LastName,
                                        birthDate = model.Birthday.ToString("yyyy-MM-dd"),
                                        email = model.EMail,
                                        mobile = model.Mobile,
                                        mobilePrefix = model.MobilePrefix,
                                        address1 = model.Address,
                                        postalCode = model.PostalCode,
                                        city = model.City,
                                        countryCode = model.CountryCode,
                                        currency = model.Currency,
                                        signupIp = ipAddress,
                                        emailVerificationURL = "http://localhost:63659/activate?key="
                                });


            return client.ExecuteTaskAsync(request)
                         .ContinueWith(task => JsonConvert.DeserializeObject(task.Result.Content));
        }

        public Task<UserLoginResponse> LoginUser(string username, string password, string ipAddress)
        {
            var client = new RestClient(ServiceUrl);
            var resourceUrl = String.Format("{0}/{1}/{2}/{3}", BuildUrl("LoginUser"),
                                            HttpUtility.UrlEncode(username),
                                            HttpUtility.UrlEncode(password),
                                            HttpUtility.UrlEncode(ipAddress));
            var request = new RestRequest(resourceUrl, Method.GET);

            return client.ExecuteTaskAsync(request)
                         .ContinueWith(task => JsonConvert.DeserializeObject<UserLoginResponse>(task.Result.Content));
        }

        public Task<object> GenerateUserHash(string url, long userId)
        {
            var client = new RestClient(ServiceUrl);
            var request = new RestRequest(BuildUrl("GenerateUserHash"), Method.POST);
            request.AddJsonBody(new
                                {
                                        URL = url,
                                        Userid = userId.ToString(CultureInfo.InvariantCulture)
                                });
            return client.ExecuteTaskAsync(request)
                         .ContinueWith(task => JsonConvert.DeserializeObject(task.Result.Content));

        }

        public Task<object> IsUserHashValid(string hashKey)
        {
            var client = new RestClient(ServiceUrl);
            var request = new RestRequest(String.Format("{0}/{1}", BuildUrl("IsUserHashValid"), hashKey), Method.GET);
            return client.ExecuteTaskAsync(request)
                         .ContinueWith(task => JsonConvert.DeserializeObject(task.Result.Content));
        }

        public Task<object> GetUserDetails(string sessionId)
        {
            var client = new RestClient(ServiceUrl);
            var request = new RestRequest(String.Format("{0}/{1}", BuildUrl("GetUserDetails"), sessionId), Method.GET);
            return client.ExecuteTaskAsync(request)
                         .ContinueWith(task => JsonConvert.DeserializeObject(task.Result.Content));
        }

        private static string BuildUrl(string method)
        {
            return String.Format("{0}/{1}/{2}/{3}", method, Version, PartnerId, PartnerKey);
        }

        private const string Version = "1.0";
        private const string PartnerId = "GreenZorroID";
        private const string PartnerKey = "GreenZorroCODE";
        private const string ServiceUrl = "http://core.gm.stage.everymatrix.com/ServerAPI/";


        
    }

    public class RegisterUserViewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }

        // Title. Set to one of : "Mr.", "Mrs.", "Miss.", "Ms."
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime Birthday { get; set; }
        public string EMail { get; set; }
        public string Mobile { get; set; }
        public string MobilePrefix { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string CountryCode { get; set; }
        
        public string Currency { get; set; }
    }

    public class LoginUserViewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class GenerateUserHashViewModel
    {
        public string Url { get; set; }
        public long UserId { get; set; }
    }

    public class TestGMServerApiController : ApiController
    {
        public TestGMServerApiController()
        {
            _gamMatrixClient = new GamMatrixClient();
        }

        [HttpGet]
        public IHttpActionResult IsUserNameAvailable(string username)
        {
            return Ok(_gamMatrixClient.IsUserNameAvailable(username).Result);
        }

        [HttpGet]
        public IHttpActionResult IsEmailAvailable(string email)
        {
            return Ok(_gamMatrixClient.IsEmailAvailable(email).Result);
        }

        [HttpGet]
        public IHttpActionResult IsAliasAvailable(string alias)
        {
            return Ok(_gamMatrixClient.IsAliasAvailable(alias).Result);
        }

        [HttpPost]
        public IHttpActionResult CheckUserExists(RegisterUserViewModel model)
        {
            return Ok(_gamMatrixClient.CheckUserExists(model.Birthday, model.EMail).Result);
        }

        // Sample Data for
        // http://localhost:63659/api/TestGMServerApi/RegisterUser
        //
        //{
        //    "Username" : "xdinos",
        //    "Password" : "12345678",
        //    "Title" : "Mr.",
        //    "FirstName" : "Dinos",
        //    "LastName" : "Chatzopoulos",
        //    "Birthday" : "1975/10/13",
        //    "Email" : "xdinos@hotmail.com",
        //    "Mobile" : "6955486367",
        //    "MobilePrefix" : "+30",
        //    "Address" : "Neofitou 48A",
        //    "PostalCode" : "34100",
        //    "City" : "Chalkida",
        //    "CountryCode" : "GR",
        //    "Currency" : "EUR"
        //}

        [HttpPost]
        public IHttpActionResult RegisterUser(RegisterUserViewModel model)
        {
            return Ok(_gamMatrixClient.RegisterUser(model, GetClientIpAddress(Request)).Result);
        }

        [HttpPost]
        public IHttpActionResult LoginUser(LoginUserViewModel model)
        {
            return Ok(_gamMatrixClient.LoginUser(model.Username, model.Password, GetClientIpAddress(Request)).Result);
        }

        [HttpPost]
        public IHttpActionResult GenerateUserHash(GenerateUserHashViewModel model)
        {
            return Ok(_gamMatrixClient.GenerateUserHash(model.Url, model.UserId).Result);
        }

        [HttpGet]
        public IHttpActionResult IsUserHashValid(string hashKey)
        {
            return Ok(_gamMatrixClient.IsUserHashValid(hashKey).Result);
        }

        [HttpGet]
        public IHttpActionResult GetUserDetails(string sessionId)
        {
            return Ok(_gamMatrixClient.GetUserDetails(sessionId).Result);
        }


        public static string GetClientIpAddress(HttpRequestMessage request)
        {
            return "94.70.20.46";
            if (request.Properties.ContainsKey(HttpContext))
            {
                dynamic ctx = request.Properties[HttpContext];
                if (ctx != null)
                    return ctx.Request.UserHostAddress;
            }

            if (request.Properties.ContainsKey(RemoteEndpointMessage))
            {
                dynamic remoteEndpoint = request.Properties[RemoteEndpointMessage];
                if (remoteEndpoint != null)
                    return remoteEndpoint.Address;
            }

            return null;
        }

        private readonly GamMatrixClient _gamMatrixClient;
        private const string HttpContext = "MS_HttpContext";
        private const string RemoteEndpointMessage = "System.ServiceModel.Channels.RemoteEndpointMessageProperty";
    }
}
