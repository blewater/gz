using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace gzWeb.Models
{
    public class Response<T> : IHttpActionResult where T : class
    {
        public ErrorViewModel Error { get; set; }
        public T Result { get; private set; }

        public Response(HttpRequestMessage request, T result)
        {
            _request = request;
            Result = result;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_request.CreateResponse(HttpStatusCode.OK, Result));
        }

        private readonly HttpRequestMessage _request;
    }

    public class ErrorViewModel
    {
        public string Message { get; set; }
        public string Details { get; set; }
    }
}