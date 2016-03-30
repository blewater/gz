using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using gzWeb.Areas.Mvc.Models;
using Newtonsoft.Json;
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

    public enum ActiveStatus
    {
        InActive = -1,
        Active = 0,
        Blocked = 1,
        Closed = 2,
    }

    public class CheckUserExistsResponse
    {
        [JsonProperty("activeStatus")]
        public ActiveStatus ActiveStatus { get; set; }

        [JsonProperty("userId")]
        public long UserId { get; set; }
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

        private static string BuildUrl(string method)
        {
            return String.Format("{0}/{1}/{2}/{3}", method, Version, PartnerId, PartnerKey);
        }

        private const string Version = "1.0";
        private const string PartnerId = "GreenZorroID";
        private const string PartnerKey = "GreenZorroCODE";
        private const string ServiceUrl = "http://core.gm.stage.everymatrix.com/ServerAPI/";
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
        public IHttpActionResult CheckUserExists(RegisterViewModel model)
        {
            return Ok(_gamMatrixClient.CheckUserExists(model.Birthday, model.Email).Result);
        }
        
        private readonly GamMatrixClient _gamMatrixClient;
    }
}
