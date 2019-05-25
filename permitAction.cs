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
using CarSharing.Exceptions;

namespace carSharing.PermitAction
{
    public static class permitAction
    {
        [FunctionName("permitAction")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            response response;

            try {
                // parse query parameter
                int vehicle_id = Convert.ToInt32 ( utilitles.getURLVar(req, "vehicle_id") );
                int user_id = Convert.ToInt32 ( utilitles.getURLVar(req, "user_id") );
                int renter_id = Convert.ToInt32 ( utilitles.getURLVar(req, "renter_id") );
                string login_hash = utilitles.getURLVar(req, "login_hash");
                int action =  Convert.ToInt32 ( utilitles.getURLVar(req, "action") );

                // Validates user identity.
                utilitles.validateUser( user_id, login_hash );
                
                if (action != 0 && action != 1)
                    throw new InvalidInputException("action");

                string[] new_status = new string[] { "DENIED", "APPROVED" };

                update_permit_status(vehicle_id, renter_id, new_status[action]);
                response = new response(1, "Permit " + new_status[action]);

            } catch (CarSharingException ex) {
                response = new response(ex.status_code, "Error: " + ex.info);
            }

            return req.CreateResponse(HttpStatusCode.OK, response, JsonMediaTypeFormatter.DefaultMediaType);
        }
        private static void update_permit_status(int vehicle_id, int renter_id, string new_status) {
            DateTime checkin = DateTime.Now;
            string update_checkin = "UPDATE Permits SET status = @status WHERE vehicle_id = @vehicle_id AND user_id = @renter_id";
            using (SqlConnection conn = new SqlConnection(_conn_str)) {
                conn.Open();
                SqlCommand command = new SqlCommand(update_checkin, conn);
                command.Parameters.AddWithValue("@status", new_status);
                command.Parameters.AddWithValue("@vehicle_id", vehicle_id);
                command.Parameters.AddWithValue("@renter_id", renter_id);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }
        private static string _conn_str = System.Environment.GetEnvironmentVariable("sqldb_connection");
    }
}
