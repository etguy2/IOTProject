using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http.Formatting;
using System.Data.SqlClient;

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

            response response;

            // Defines tiem time treshold for the permit
            DateTime checkin_expiration_treshold = DateTime.Now;
            checkin_expiration_treshold.AddHours(-3); // Subs 3 hours because Azure are stupid.
            checkin_expiration_treshold.AddMinutes(-1*checkin_validity_time);

            bool status = false;

            // Look for the right checkin in the DB
            string checkin_query =  "SELECT COUNT(*) FROM Permits "
                                    + "INNER JOIN Vehicles ON Vehicles.id = Permits.vehicle_id "
                                    + "INNER JOIN Devices ON Vehicles.device_id = Devices.id AND Devices.MACID = @macid"
                                    + "AND Permits.checkin >= Convert(datetime, @checkin_expiration_treshold )";

            using (SqlConnection conn = new SqlConnection(_conn_str)) {
                conn.Open();
                SqlCommand command = new SqlCommand(checkin_query, conn);
                command.Parameters.AddWithValue("@macid", macid);
                int rows = (int) command.ExecuteScalar();
                if (rows >= 1) 
                    status = true;
                
                conn.Close();
            }

            if (status) {
                response = new response(1, "Approved");

            } else {
                response = new response(-1, "No permit");
            }


            return req.CreateResponse(HttpStatusCode.OK, response, JsonMediaTypeFormatter.DefaultMediaType);
        }
        private static string _conn_str = System.Environment.GetEnvironmentVariable("sqldb_connection");
    }
}
