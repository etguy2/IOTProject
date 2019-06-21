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

namespace CarSharing.getUserAllHistory
{
    public static class getUserAllHistory
    {
        [FunctionName("getUserAllHistory")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            response response = new response(0, "Error");
            History r_response = new History();
            try {
                // parse query parameter
                int user_id = Convert.ToInt32(utilitles.getURLVar(req, "user_id"));
                string login_hash = utilitles.getURLVar(req, "login_hash");

                // Validates user identity.
                utilitles.validateUser( user_id , login_hash );

                response.status = 1;
                r_response.status = 1;

                r_response.events = getEventList(user_id);

            } catch (CarSharingException ex) {
                response = new response(ex.status_code, "Error: " + ex.info);
            }

            return response.status == 1 ? req.CreateResponse(HttpStatusCode.OK, r_response, JsonMediaTypeFormatter.DefaultMediaType)
            : req.CreateResponse(HttpStatusCode.OK, response, JsonMediaTypeFormatter.DefaultMediaType);
        }
        private static List<History_row> getEventList(int user_id) {
            List<History_row> Events = new List<History_row>();
            using (SqlConnection conn = new SqlConnection(_conn_str)) {
                conn.Open();
                string avg_review = "SELECT hisDate, hisCost, hisLisence FROM History WHERE user_id = @user_id ORDER BY id DESC";
                SqlCommand command = new SqlCommand(avg_review, conn);
                command.Parameters.AddWithValue("@user_id", user_id);
                using (SqlDataReader reader = command.ExecuteReader()) {               
                    while (reader.Read()) {
                        Events.Add(new History_row((int)reader["hisCost"],(string)reader["hisDate"],(string)reader["hisLisence"]));
                    }  
                }
                conn.Close();
            }
            return Events;
        }
        private static string _conn_str = System.Environment.GetEnvironmentVariable("sqldb_connection");
        private class History_row {
            public int hisCost;
            public string hisDate;
            public string hisLisence;
            public History_row(int hisCost, string hisDate, string hisLisence) {
                this.hisCost = hisCost;
                this.hisDate = hisDate;
                this.hisLisence = hisLisence;
            }
        }
        private class History {
            public int status;
            public List<History_row> events;

        }
    }
}
