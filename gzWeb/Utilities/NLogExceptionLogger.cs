using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Web.Http.ExceptionHandling;

namespace gzWeb.Utilities
{
    public class NLogExceptionLogger : ExceptionLogger
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public override void Log(ExceptionLoggerContext context)
        {
            var logEventInfo = new NLog.LogEventInfo(NLog.LogLevel.Error, Logger.Name,
                                                     CultureInfo.InvariantCulture,
                                                     RequestToString(context.Request),
                                                     null,
                                                     context.Exception);
            //logEventInfo.Properties[""];
            Logger.Log(GetType(), logEventInfo);
        }

        private static string RequestToString(HttpRequestMessage request)
        {
            var message = new StringBuilder();
            if (request.Method != null)
                message.Append(request.Method);

            if (request.RequestUri != null)
                message.Append(" ").Append(request.RequestUri);

            return message.ToString();
        }
    }
}

