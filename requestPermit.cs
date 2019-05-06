using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http.Formatting;
using System.Data.SqlClient;

namespace carSharing.requestPermit
{
    public static class requestPermit
    {
        private static string _conn_str = System.Environment.GetEnvironmentVariable("sqldb_connection");

        [FunctionName("requestPermit")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            response response = new response(0, "Error");

            try {
                // parse query parameter.
                string user_id = utilitles.getURLVar(req, "user_id");
                string login_hash = utilitles.getURLVar(req, "login_hash");
                string vehicle_id = utilitles.getURLVar(req, "vehicle_id");

                // Validates user identity.
                utilitles.validateUser( System.Convert.ToInt32( user_id ) , login_hash );

                // creates the request.
                createPermit(user_id, vehicle_id);

                // Notify the owner of the car.
                int owner_id = utilitles.getOwnerByVehicle( vehicle_id );
                utilitles.notifyUserById("Car Request", "Someone has requested your car no. " + vehicle_id, owner_id);
                response = new response(1, "Permit request created");

            } catch (CarSharingException ex) {
                response = new response(ex.status, ex.Message);
            }

            // Send back the response for the app.
            return req.CreateResponse(HttpStatusCode.OK, response, JsonMediaTypeFormatter.DefaultMediaType);   
        
        }
        public static void createPermit(string user_id, string vehicle_id) {
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
