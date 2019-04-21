using System.Linq;
using System.Collections.Generic; 
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace CarSharing.carHandler
{
    public static class carHandler
    {
        [FunctionName("carHandler")]
        
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            string target = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "target", true) == 0)
                .Value;
            string action = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "action", true) == 0)
                .Value;

            string validation_string = utilitles.RandomString(8);
            //string action = "unlock";
            //+972587559289
            sms unlock_signal =  new sms(action, target, validation_string);
            unlock_signal.send();
            return req.CreateResponse(HttpStatusCode.OK, unlock_signal._last_response);
        }
        private static readonly HttpClient client = new HttpClient();
    }
}
