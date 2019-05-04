using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace carSharing.deviceCarAction
{
    public static class deviceCarAction
    {
        [FunctionName("deviceCarAction")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {

            // parse query parameter
            string macid = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "macid", true) == 0)
                .Value;


            return req.CreateResponse(HttpStatusCode.OK, "Hello ");
        }
    }
}
