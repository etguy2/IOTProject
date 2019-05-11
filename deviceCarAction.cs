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
            string macid = utilitles.getURLVar(req, "macid");

            int response;

            // Makes sure the user has matched all the restrictions to unlock the car.
            bool status = verifyCheckin( formatMACID( macid ) );

            response = (status) ? 1 : 0;

            return req.CreateResponse(HttpStatusCode.OK, status.ToString(), "text/plain");
        }

        // Reversing the URL format of the MACID
        private static string formatMACID(string macid) {
            return macid.Replace('_', ':');
        }
        private static string notifyOwner(string macid) {
            string get_user_query = "SELECT Vehicles.owner_id, Vehicles.id as vehicle_id, Users.id as user_id, Users.FirstName, Users.LastName FROM Vehicles "
                                    + "INNER JOIN Permits ON Permits.status = 'APPROVED'"
                                    + "INNER JOIN Devices ON Devices.MACID = @macid AND Vehicles.device_id = Devices.id "
                                    + "INNER JOIN Users ON Users.id = Permits.user_id";

            using (SqlConnection conn = new SqlConnection(_conn_str)) {
                conn.Open();
                SqlCommand command = new SqlCommand(get_user_query, conn);
                command.Parameters.AddWithValue("@macid", macid);
                using (SqlDataReader reader = command.ExecuteReader()) {               
                    if (reader.Read()) {
                        string name = (string)reader["FirstName"] + " " + (string)reader["LastName"];
                        data notify_data = new data(3, (int)reader["user_id"], (int)reader["vehicle_id"]);
                        return utilitles.notifyUserById("Car Unlocked", name + " Has just unlocked your car", (int)reader["owner_id"], notify_data);
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
                command.Parameters.AddWithValue("@macid", macid);
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
