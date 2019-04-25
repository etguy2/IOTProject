using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Collections.Generic;
using System.Net.Http.Formatting;

namespace CarSharing.getAvailableCars
{
    public static class getAvailableCars
    {
        [FunctionName("getAvailableCars")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {

            // parse query parameter
            string name = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
                .Value;

            car c1;
            car c2;
            List<car> cars = new List<car>();

            c1.id = "3476934";
            c1.lat = "32.778052";
            c1.lng = "35.021980";
            c2.id = "3477734";
            c2.lat = "32.778205";
            c2.lng = "35.021779";

            cars.Add(c1);
            cars.Add(c2);


            

            return req.CreateResponse(HttpStatusCode.OK, cars, JsonMediaTypeFormatter.DefaultMediaType);
        }
        public struct car {
            public string id;
            public string lat;
            public string lng;
        }
    }
}
