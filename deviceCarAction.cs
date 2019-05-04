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
            // parse query parameter
            string macid = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "macid", true) == 0)
                .Value;

            response response;
            bool status = verifyCheckin(macid);

            if (status)
                response = new response(1, "Approved");
            else 
                response = new response(-1, "No permit");

            return req.CreateResponse(HttpStatusCode.OK, response, JsonMediaTypeFormatter.DefaultMediaType);
        }

        private static string notifyOwner(string macid) {
            string get_user_query = "SELECT Vehicles.owner_id, Users.FirstName, Users.LastName FROM Vehicles "
                                    + "CROSS JOIN Permits "
                                    + "INNER JOIN Devices ON Devices.MACID = @macid AND Vehicles.device_id = Devices.id "
                                    + "INNER JOIN Users ON Users.id = Permits.user_id";

             using (SqlConnection conn = new SqlConnection(_conn_str)) {
                conn.Open();
                SqlCommand command = new SqlCommand(get_user_query, conn);
                command.Parameters.AddWithValue("@macid", Convert.ToInt32(macid));
                using (SqlDataReader reader = command.ExecuteReader()) {               
                    if (reader.Read()) {
                        string name = (string)reader["FirstName"] + " " + (string)reader["LastName"];
                        return utilitles.notifyUserById("Car Unlocked", name + " Has just unlocked your car", (int)reader["owner_id"]);
                    }
                }
               
                
                conn.Close();
                return "";
            }
        }

        private static bool verifyCheckin(string macid) {
            bool status = false;
            // Defines tiem time treshold for the permit
            DateTime checkin_expiration_treshold = DateTime.Now;
            checkin_expiration_treshold = checkin_expiration_treshold.AddMinutes(-1*_checkin_validity_time);
            // Look for the right checkin in the DB
            string checkin_query =  "SELECT COUNT(*) FROM Permits "
                                    + "INNER JOIN Vehicles ON Vehicles.id = Permits.vehicle_id "
                                    + "INNER JOIN Devices ON Vehicles.device_id = Devices.id AND Devices.MACID = @macid "
                                    + "AND Permits.checkin >= Convert(datetime, @checkin_expiration_treshold )";

            using (SqlConnection conn = new SqlConnection(_conn_str)) {
                conn.Open();
                SqlCommand command = new SqlCommand(checkin_query, conn);
                command.Parameters.AddWithValue("@macid", Convert.ToInt32(macid));
                command.Parameters.AddWithValue("@checkin_expiration_treshold", checkin_expiration_treshold);
                int rows = (int) command.ExecuteScalar();
                if (rows >= 1) {
                    status = true;
                    notifyOwner(macid);
                }
                
                conn.Close();
            }
            return status;
        }
        private static string _conn_str = System.Environment.GetEnvironmentVariable("sqldb_connection");
        private static int _checkin_validity_time = Convert.ToInt32(System.Environment.GetEnvironmentVariable("checkin_expiration"));
    }
}
