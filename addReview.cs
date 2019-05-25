using System;
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

namespace carSharing.addReview
{
    public static class addReview
    {
        [FunctionName("addReview")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
             response response = new response(0, "Error");
            try {
                // parse query parameter
                int reviewer_id = Convert.ToInt32(utilitles.getURLVar(req, "reviewer_id"));
                int reviewee_id = Convert.ToInt32(utilitles.getURLVar(req, "reviewee_id"));
                int rate = Convert.ToInt32(utilitles.getURLVar(req, "rate"));
                string cont = utilitles.getURLVar(req, "cont");
                string login_hash = utilitles.getURLVar(req, "login_hash");

                // Validates user identity.
                utilitles.validateUser( reviewer_id , login_hash );

                insertReview(reviewer_id, reviewee_id, rate, cont);

                response = new response(1, "Review added");

            } catch (CarSharingException ex) {
                response = new response(ex.status_code, "Error: " + ex.info);
            }

            return req.CreateResponse(HttpStatusCode.OK, response, JsonMediaTypeFormatter.DefaultMediaType);
        }
        private static void insertReview(int reviewer_id, int reviewee_id, int rate, string cont) {
            using (SqlConnection conn = new SqlConnection(_conn_str)) {
                conn.Open();
                string create_permit = "INSERT INTO Reviews  (reviewer_id, reviewee_id, rate, cont) values (@reviewer_id, @reviewee_id, @rate, @cont)";
                SqlCommand command = new SqlCommand(create_permit, conn);
                command.Parameters.AddWithValue("@reviewer_id", reviewer_id);
                command.Parameters.AddWithValue("@reviewee_id", reviewee_id);
                command.Parameters.AddWithValue("@rate", rate);
                command.Parameters.AddWithValue("@cont", cont);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }
        private static string _conn_str = System.Environment.GetEnvironmentVariable("sqldb_connection");
    }
}
