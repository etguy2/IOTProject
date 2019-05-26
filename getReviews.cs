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

namespace CarSharing.getReviews
{
    public static class getReviews
    {
        [FunctionName("getReviews")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            response response = new response(0, "Error");
            Reviews_response r_response = new Reviews_response();
            try {
                // parse query parameter
                int user_id = Convert.ToInt32(utilitles.getURLVar(req, "user_id"));
                string login_hash = utilitles.getURLVar(req, "login_hash");
                int reviewee_id = Convert.ToInt32(utilitles.getURLVar(req, "reviewee_id"));

                // Validates user identity.
                utilitles.validateUser( user_id , login_hash );

                response.status = 1;
                r_response.status = 1;

                r_response.avg_rate = getAvgReview(reviewee_id);
                r_response.reviews = getReviewList(reviewee_id);

            } catch (CarSharingException ex) {
                response = new response(ex.status_code, "Error: " + ex.info);
            }

            return response.status == 1 ? req.CreateResponse(HttpStatusCode.OK, r_response, JsonMediaTypeFormatter.DefaultMediaType)
            : req.CreateResponse(HttpStatusCode.OK, response, JsonMediaTypeFormatter.DefaultMediaType);
        }
        private static double getAvgReview(int reviewee_id) {
            using (SqlConnection conn = new SqlConnection(_conn_str)) {
                conn.Open();
                string avg_review = "SELECT AVG(rate) FROM Reviews WHERE reviewee_id = @reviewee_id";
                SqlCommand command = new SqlCommand(avg_review, conn);
                command.Parameters.AddWithValue("@reviewee_id", reviewee_id);
                double avg = (double)(int) command.ExecuteScalar();
                conn.Close();
                return avg;
            }
        }
        private static List<Review> getReviewList(int reviewee_id) {
            List<Review> Reviews = new List<Review>();
            using (SqlConnection conn = new SqlConnection(_conn_str)) {
                conn.Open();
                string avg_review = "SELECT cont, rate, reg_time FROM Reviews WHERE reviewee_id = @reviewee_id";
                SqlCommand command = new SqlCommand(avg_review, conn);
                command.Parameters.AddWithValue("@reviewee_id", reviewee_id);
                using (SqlDataReader reader = command.ExecuteReader()) {               
                    while (reader.Read()) {
                        Reviews.Add(new Review((int)reader["rate"],(string)reader["cont"], (DateTime)reader["reg_time"]));
                    }  
                }
                conn.Close();
            }
            return Reviews;
        }
        private static string _conn_str = System.Environment.GetEnvironmentVariable("sqldb_connection");
        private class Review {
            public int rate;
            public string content;
            public DateTime reg_time;
            public Review(int rate, string content, DateTime reg_time) {
                this.rate = rate;
                this.content = content;
                this.reg_time = reg_time;
            }
        }
        private class Reviews_response : response {
            public double avg_rate;
            public List<Review> reviews;

        }
    }
}
