using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace gzWeb.Utilities
{
    public class OkWithMessageResult : IHttpActionResult
    {
        #region Constructor
        public OkWithMessageResult(HttpRequestMessage request, Response response)
        {
            _request = request;
            _response = response;
        }
        #endregion

        #region Implementation of IHttpActionResult
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            //var response = new HttpResponseMessage(HttpStatusCode.OK) {
            //    Content = new ObjectContent<Response>(_response, new JsonMediaTypeFormatter(), "application/json")
            //};
            var response = _request.CreateResponse(HttpStatusCode.OK, _response);
            return Task.FromResult(response);
        }
        #endregion

        #region Fields
        private readonly HttpRequestMessage _request;
        private readonly Response _response;
        #endregion
    }
}