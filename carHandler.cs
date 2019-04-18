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
            
           string sms_key = System.Environment.GetEnvironmentVariable("sms_key");
           var values = new Dictionary<string, string>
            {
                { "from", "Guy" },
                { "text", "Hello" },
                { "to", "972544454162" },
                { "api_key", "513e3c1c" },
                { "api_secret", sms_key }
            };
            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync("https://rest.nexmo.com/sms/json", content);

            var responseString = await response.Content.ReadAsStringAsync();
            return req.CreateResponse(HttpStatusCode.OK, responseString);
        }
        private static readonly HttpClient client = new HttpClient();
    }
}
