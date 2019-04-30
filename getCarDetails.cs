using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Data.SqlClient;
using System.Net.Http.Formatting;

namespace CarSharing.getCarDetails
{
    public static class getCarDetails
    {
        [FunctionName("getCarDetails")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            // parse query parameter
            string vehicle_id = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "vehicle_id", true) == 0)
                .Value;

            string get_car_query =  "SELECT Vehicles.id as id, Devices.lat, Devices.lng, Users.FirstName as owner_first_name, Users.LastName as owner_last_name, "
                +"Vehicles.model, Vehicles.mode, Users.email as owneremail, Vehicles.img as carimage, Users.id as ownerid,  Vehicles.manufacturer "
                +"FROM Devices "
                +"INNER JOIN Vehicles ON Vehicles.device_id = Devices.id "
                +"INNER JOIN Users ON Users.id = Vehicles.owner_id AND Vehicles.id = @vehicle_id";

            string _conn_str = System.Environment.GetEnvironmentVariable("sqldb_connection");
            bool success = false;
            car car = new car();
            response bad_response = new response(-1, "bad car id");
            using (SqlConnection conn = new SqlConnection(_conn_str)) {
                conn.Open();
                SqlCommand command = new SqlCommand(get_car_query, conn);
                command.Parameters.AddWithValue("@vehicle_id", vehicle_id);
                using (SqlDataReader reader = command.ExecuteReader()) {               
                    if (reader.Read()) {
                        car = new car(reader);
                        success = true;
                    }
                }
                conn.Close();
            }

            if (success) {
                return req.CreateResponse(HttpStatusCode.OK, car, JsonMediaTypeFormatter.DefaultMediaType);
            } else {
                return req.CreateResponse(HttpStatusCode.OK, bad_response, JsonMediaTypeFormatter.DefaultMediaType);
            }
            
        }
    }
}
