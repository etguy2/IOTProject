using System.Linq;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using CarSharing.Cryptography;
using System.Data.SqlClient;
using System.Net.Http.Formatting;

namespace CarSharing.Login
{
    public static class Login
    {
        [FunctionName("Login")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            // Parse query parameter
            string email = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "email", true) == 0)
                .Value;
            string password = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "password", true) == 0)
                .Value;
            string notification_token = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "token", true) == 0)
                .Value;
                
            // Task vars 
            string _conn_str = System.Environment.GetEnvironmentVariable("sqldb_connection");
            bool login_success = false;

            // Get user from the DB
            string input_pass = SHA.GenerateSHA256String(password).ToLower(); // Encrypc input_password (SHA256)
            string query =  "SELECT top 1 password_enc, enc_string, id FROM Users WHERE email = @email";
            
            login_response res = new login_response(-1, 0, null); // Default bad login response
            using (SqlConnection conn = new SqlConnection(_conn_str)) {
                conn.Open();
                SqlCommand get_cmd = new SqlCommand(query, conn);
                get_cmd.Prepare();
                get_cmd.Parameters.Add("@email", SqlDbType.NVarChar, 50);
                get_cmd.Parameters["@email"].Value = email; 
                string enc_string = utilitles.RandomString(5); // Generate a new enc_string.
                using (SqlDataReader reader = get_cmd.ExecuteReader()) {
                    if (reader.Read()) {  
                        if (string.Compare(input_pass, (string)reader["password_enc"]) == 0) { // Checks if the 2 encrypted passwords match.
                            login_success = true;
                            string update_token_query =  "UPDATE Users SET notification_token = '@notification_token' WHERE id = @user_id";
                            SqlCommand update_token_cmd = new SqlCommand(update_token_query, conn);
                            update_token_cmd.Prepare();
                            update_token_cmd.Parameters.Add("@notification_token", SqlDbType.NVarChar, 50);
                            update_token_cmd.Parameters["@notification_token"].Value = notification_token;
                            update_token_cmd.Parameters.Add("@user_id", SqlDbType.Int);
                            update_token_cmd.Parameters["@user_id"].Value = (int)reader["id"];
                            update_token_cmd.ExecuteNonQuery();
                            string login_hash = SHA.GenerateSHA256String(input_pass+enc_string).ToLower(); // Generate login hash.
                            res = new login_response(1, (int)reader["id"], login_hash); // Generate login response.
                        }   
                    }
                }
                if (login_success) { // Update the new enc_string in DB.
                    string update_enc_string =  "UPDATE Users SET enc_string = '"+enc_string+"' WHERE email = '"+email+"'"; 
                    SqlCommand update_cmd = new SqlCommand(update_enc_string, conn);
                    update_cmd.ExecuteNonQuery();
                }
                conn.Close();
            }

            return req.CreateResponse(HttpStatusCode.OK, res, JsonMediaTypeFormatter.DefaultMediaType);
        }
    }
}
