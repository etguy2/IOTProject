using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using CarSharing.Exceptions;
using System.Net.Http.Formatting;
using System.Data.SqlClient;
using System.Collections.Generic;
using System;

namespace CarSharing.setVehicleStatus
{
    public static class setVehicleStatus
    {
        [FunctionName("setVehicleStatus")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            response response = new response(0, "Error");
            try {
                // parse query parameter
                int user_id = Convert.ToInt32(utilitles.getURLVar(req, "user_id"));
                string login_hash = utilitles.getURLVar(req, "login_hash");
                int vehicle_id = Convert.ToInt32(utilitles.getURLVar(req, "vehicle_id"));
                string new_status = utilitles.getURLVar(req, "status"); // 'ACTIVATED', 'DEACTIVATED', 'IN-USE'

                // Validates user identity.
                utilitles.validateUser( user_id , login_hash );
                update_vehicle_status(vehicle_id, new_status);
                response.status = 1;
                response.description = "status changed to " + new_status;

            } catch (CarSharingException ex) {
                response = new response(ex.status_code, "Error: " + ex.info);
            }

            return req.CreateResponse(HttpStatusCode.OK, response, JsonMediaTypeFormatter.DefaultMediaType);
        }
        private static void update_vehicle_status(int vehicle_id, string new_status) {
            string update_status = "UPDATE Vehicles SET status = @status WHERE id = @vehicle_id";
            using (SqlConnection conn = new SqlConnection(_conn_str)) {
                conn.Open();
                SqlCommand command = new SqlCommand(update_status, conn);
                command.Parameters.AddWithValue("@status", format_status(new_status));
                command.Parameters.AddWithValue("@vehicle_id", vehicle_id);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }
        private static string format_status(string status) {
            switch(status) {
                case "ACTIVATED":
                case "DEACTIVATED":
                case "IN-USE":
                    return status;
                default:
                    throw new InvalidInputException(status);
            }
        }
        private static string _conn_str = System.Environment.GetEnvironmentVariable("sqldb_connection");
    }
}
