using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http.Formatting;
using System.Collections.Generic;

namespace CarSharing.LogHandler
{
    public static class LogHandler
    {
        [FunctionName("LogHandler")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            string limit = req.GetQueryNameValuePairs()
            .FirstOrDefault(q => string.Compare(q.Key, "limit", true) == 0)
            .Value;
            string action = req.GetQueryNameValuePairs()
            .FirstOrDefault(q => string.Compare(q.Key, "action", true) == 0)
            .Value;
            string message = req.GetQueryNameValuePairs()
            .FirstOrDefault(q => string.Compare(q.Key, "message", true) == 0)
            .Value;
            dbConnect db = new dbConnect ();
            if (action == "get_logs") {
                List<log> logs = db.get_logs(limit);
                return req.CreateResponse(HttpStatusCode.OK, logs, JsonMediaTypeFormatter.DefaultMediaType);
            } else {
                db.insert_log(message);
                response res = new response(1, "Log Added2");
                return req.CreateResponse(HttpStatusCode.OK, res, JsonMediaTypeFormatter.DefaultMediaType);
            }
        }
    }
}
