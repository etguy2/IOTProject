using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Security.Cryptography;  
using CarSharing.Cryptography;
using System.Net.Http.Formatting;

namespace CarSharing.getUserDetails
{
    public static class getUserDetails
    {
        [FunctionName("getUserDetails")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // parse query parameter
            string user_id = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "user_id", true) == 0)
                .Value;

            string query =  "SELECT top 1 id, FirstName, LastName, email, password_enc, enc_string, licence_number FROM Users WHERE id = @user_id";
            
            string _conn_str = System.Environment.GetEnvironmentVariable("sqldb_connection");
            using (SqlConnection conn = new SqlConnection(_conn_str)) {
                conn.Open();
                SqlCommand command = new SqlCommand(query, conn);
                command.Parameters.AddWithValue("@user_id", user_id);
                using (SqlDataReader reader = command.ExecuteReader()) {               
                    if (reader.Read()) {

                        user usr = new user((int)reader["id"],
                            (string)reader["FirstName"],
                            (string)reader["LastName"], 
                            (string)reader["email"],
                            (string)reader["licence_number"], "");
                            usr.setPermits( getPermitsByUser( Convert.ToInt32(user_id) ) );
                            return req.CreateResponse(HttpStatusCode.OK, usr, JsonMediaTypeFormatter.DefaultMediaType);
                    }  
                }
                conn.Close();
            }
            response res2 = new response(-2, "ERROR (Unknown)");
            return req.CreateResponse(HttpStatusCode.BadRequest, res2, JsonMediaTypeFormatter.DefaultMediaType);
        }
        private static List<Permit_small> getPermitsByUser(int user_id) {
            string query =  "SELECT vehicle_id, time, status FROM Permits WHERE user_id = @user_id";
            List<Permit_small> permits = new List<Permit_small>();

            string _conn_str = System.Environment.GetEnvironmentVariable("sqldb_connection");
            using (SqlConnection conn = new SqlConnection(_conn_str)) {
                conn.Open();
                SqlCommand command = new SqlCommand(query, conn);
                command.Parameters.AddWithValue("@user_id", user_id);
                using (SqlDataReader reader = command.ExecuteReader()) {  
                    while (reader.Read()) {
                        permits.Add(new Permit_small((int)reader["vehicle_id"], (string)reader["status"], (DateTime)reader["time"]));
                    }
                }
                conn.Close();
            }
            return permits;
        }
    }
}
