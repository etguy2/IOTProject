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

namespace CarSharing.myCars
{
    public static class myCars
    {
        [FunctionName("myCars")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            response response = new response(0, "Error");
            List<Car> Cars = new List<Car>();
            try {
                // parse query parameter
                int user_id = Convert.ToInt32(utilitles.getURLVar(req, "user_id"));
                string login_hash = utilitles.getURLVar(req, "login_hash");

                // Validates user identity.
                utilitles.validateUser( user_id , login_hash );

                response.status = 1;
                Cars = getCarsList(user_id);


            } catch (CarSharingException ex) {
                response = new response(ex.status_code, "Error: " + ex.info);
            }

            return response.status == 1 ? req.CreateResponse(HttpStatusCode.OK, Cars, JsonMediaTypeFormatter.DefaultMediaType)
            : req.CreateResponse(HttpStatusCode.OK, response, JsonMediaTypeFormatter.DefaultMediaType);
        }
        private static List<Car> getCarsList(int owner_id) {
            List<Car> Cars = new List<Car>();
            using (SqlConnection conn = new SqlConnection(_conn_str)) {
                conn.Open();
                string getCars = "SELECT id, manufacturer, mode, model, prod_year FROM Vehicles WHERE owner_id = @owner_id ORDER BY id DESC";
                SqlCommand command = new SqlCommand(getCars, conn);
                command.Parameters.AddWithValue("@owner_id", owner_id);
                using (SqlDataReader reader = command.ExecuteReader()) {               
                    while (reader.Read()) {
                        Cars.Add(new Car(reader));
                    }  
                }
                conn.Close();
            }
            return Cars;
        }
        private static string _conn_str = System.Environment.GetEnvironmentVariable("sqldb_connection");
        private class Car {
            public int id;
            public int prod_year;
            public string manufacturer;
            public int mode;
            public string model;
            public Car(SqlDataReader reader) {
                this.id = (int)reader["id"];
                this.prod_year = (int)reader["prod_year"];
                this.manufacturer = (string)reader["manufacturer"];
                this.mode = (int)reader["mode"];
                this.model = (string)reader["model"];
            }
        }
    }
}
