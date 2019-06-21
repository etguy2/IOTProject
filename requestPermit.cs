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

namespace carSharing.requestPermit
{
    public static class requestPermit
    {
        private static string _conn_str = System.Environment.GetEnvironmentVariable("sqldb_connection");

        [FunctionName("requestPermit")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            response response = new response(0, "Error");
            HttpStatusCode sc = HttpStatusCode.OK;

            try {
                // parse query parameter.
                string user_id = utilitles.getURLVar(req, "user_id");
                string login_hash = utilitles.getURLVar(req, "login_hash");
                string vehicle_id = utilitles.getURLVar(req, "vehicle_id");

                // Validates user identity.
                utilitles.validateUser( System.Convert.ToInt32( user_id ) , login_hash );

                // Makes sure there are no simillar Permit requests on the DB
                lookForSimilarPermits(Convert.ToInt32(user_id), Convert.ToInt32(vehicle_id));

                // creates the request.
                createPermit(user_id, vehicle_id);

                // Notify the owner of the car.
                int owner_id = utilitles.getOwnerByVehicle( vehicle_id );
                data notify_data = new data(1, Convert.ToInt32(user_id), Convert.ToInt32(vehicle_id), "");
                string username = utilitles.getUsernameById( Convert.ToInt32( user_id ) );

                utilitles.notifyUserById("Car Request", username + " has requested your car no. " + vehicle_id, owner_id, notify_data);

                response = new response(1, "Permit request created");

            } catch (CarSharingException ex) {
                response = new response(ex.status_code, "Error: " + ex.info);
            }

            return req.CreateResponse(HttpStatusCode.OK, response, JsonMediaTypeFormatter.DefaultMediaType);
        }
        private static void lookForSimilarPermits(int user_id, int vehicle_id) {
            bool status = true;

            // Defines tiem time treshold for the permit
            DateTime permit_expiration_treshold = DateTime.Now;
            permit_expiration_treshold = permit_expiration_treshold.AddMinutes( -1 * _permit_validity_time);

            string look_for_similar = "SELECT COUNT(*) FROM Permits WHERE user_id = @user_id AND vehicle_id = @vehicle_id AND Permits.time >= Convert(datetime, @permit_expiration_treshold )";
            using ( SqlConnection conn = new SqlConnection( _conn_str ) ) {
                conn.Open();
                SqlCommand command = new SqlCommand( look_for_similar, conn );
                command.Parameters.AddWithValue("@user_id", user_id);
                command.Parameters.AddWithValue("@vehicle_id", vehicle_id);
                command.Parameters.AddWithValue("@permit_expiration_treshold", permit_expiration_treshold);

                int rows = (int) command.ExecuteScalar();
                if (rows >= 1) 
                    status = false;
                
                conn.Close();
            }
            if (!status) throw new PermitRequestExists(user_id, vehicle_id);
        }
        private static int _permit_validity_time = Convert.ToInt32(System.Environment.GetEnvironmentVariable("permit_expiration"));

        private static void createPermit(string user_id, string vehicle_id) {
            using (SqlConnection conn = new SqlConnection(_conn_str)) {
                conn.Open();
                string create_permit = "INSERT INTO Permits  (user_id, vehicle_id, status) values (@user_id, @vehicle_id, 'WAITING')";
                SqlCommand command = new SqlCommand(create_permit, conn);
                command.Parameters.AddWithValue("@user_id", user_id);
                command.Parameters.AddWithValue("@vehicle_id", vehicle_id);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }
    }
}
