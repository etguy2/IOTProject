using System;
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
            
            // Const Vars
            const int checkin_validity_time = 10; // No. of minutes until permit expiration time.

            // parse query parameter
            string macid = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "macid", true) == 0)
                .Value;

            // Defines tiem time treshold for the permit
            DateTime checkin_expiration_treshold = DateTime.Now;
            checkin_expiration_treshold.AddHours(-3); // Subs 3 hours because Azure are stupid.
            checkin_expiration_treshold.AddMinutes(-1*checkin_validity_time);

            // Look for the right checkin in the DB
            string checkin_query =  "SELECT COUNT(*) FROM Permits "
                                    + "INNER JOIN Vehicles ON Vehicles.id = Permits.vehicle_id "
                                    + "INNER JOIN Devices ON Vehicles.device_id = Devices.id AND Devices.MACID = @macid"
                                    + "AND Permits.checkin >= Convert(datetime, @checkin_expiration_treshold )";





            return req.CreateResponse(HttpStatusCode.OK, "Hello ");
        }
    }
}
