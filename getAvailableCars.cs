using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Data.SqlClient;

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
            string get_cars_query =  "SELECT Vehicles.id as id, Vehicles.prod_year as year, Devices.lat, Devices.lng, "
                +"Vehicles.model, Vehicles.mode, Users.id as ownerid, Vehicles.manufacturer "
                +"FROM Devices "
                +"INNER JOIN Vehicles ON Vehicles.device_id = Devices.id "
                +"INNER JOIN Users ON Users.id = Vehicles.owner_id";

            // Task vars 
            string _conn_str = System.Environment.GetEnvironmentVariable("sqldb_connection");
            List<car> cars = new List<car>();
            using (SqlConnection conn = new SqlConnection(_conn_str)) {
                conn.Open();
                SqlCommand command = new SqlCommand(get_cars_query, conn);
                using (SqlDataReader reader = command.ExecuteReader()) {                
                    while (reader.Read()) {
                        cars.Add(new car(reader));
                    }
                }
                conn.Close();
             }
            

            return req.CreateResponse(HttpStatusCode.OK, cars, JsonMediaTypeFormatter.DefaultMediaType);
        }
    }
}
