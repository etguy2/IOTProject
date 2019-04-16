using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Data.SqlClient;
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
            string login_hash = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "login_hash", true) == 0)
                .Value;
            string user_id = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "user_id", true) == 0)
                .Value;

            string query =  "SELECT top 1 id, FirstName, LastName, email, password_enc, enc_string, licence_number FROM Users WHERE id = '"+user_id+"'";
            string _conn_str = System.Environment.GetEnvironmentVariable("sqldb_connection");
            using (SqlConnection conn = new SqlConnection(_conn_str)) {
                conn.Open();
                SqlCommand command = new SqlCommand(query, conn);
                using (SqlDataReader reader = command.ExecuteReader()) {               
                    if (reader.Read()) {
                        string user_hash = SHA.GenerateSHA256String((string)reader["password_enc"]+(string)reader["enc_string"]).ToLower();
                        if (string.Compare(user_hash, login_hash) == 0) {
                            user usr = new user((int)reader["id"],
                                (string)reader["FirstName"],
                                (string)reader["LastName"], 
                                (string)reader["email"],
                                (string)reader["licence_number"]);
                                return req.CreateResponse(HttpStatusCode.OK, usr, JsonMediaTypeFormatter.DefaultMediaType);
                        } else {
                            response res = new response(-1, "Bad Login Hash");
                            return req.CreateResponse(HttpStatusCode.BadRequest, res, JsonMediaTypeFormatter.DefaultMediaType);
                        }
                    }  
                }
                conn.Close();
            }
            response res2 = new response(-2, "ERROR (Unknown)");
            return req.CreateResponse(HttpStatusCode.BadRequest, res2, JsonMediaTypeFormatter.DefaultMediaType);
        }
    }
}
