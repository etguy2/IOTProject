using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net.Http.Formatting;
using CarSharing.Exceptions;

namespace carSharing.getPending
{
    public static class getPending
    {
        [FunctionName("getPending")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            try {

                // parse query parameter
                int user_id = Convert.ToInt32 ( utilitles.getURLVar(req, "user_id") );
                string login_hash = utilitles.getURLVar(req, "login_hash");

                // Validates user identity.
                utilitles.validateUser( user_id, login_hash );

                List<Permit> waiting_permits = getPermits(user_id, "WAITING");
                List<Permit> approved_permits = getPermits(user_id, "APPROVED");

                PermitList permits = new PermitList(waiting_permits, approved_permits);
                return req.CreateResponse(HttpStatusCode.OK, permits, JsonMediaTypeFormatter.DefaultMediaType);

            } catch (CarSharingException ex) {
                response response =  new response(ex.status_code, "ERROR: " + ex.info);
                return req.CreateResponse(HttpStatusCode.OK, response, JsonMediaTypeFormatter.DefaultMediaType);
            }
        }
        public static List<Permit> getPermits(int user_id, string status){
            List<Permit> permits =  new List<Permit>();
            string get_permits = "SELECT "
                    + "Permits.id as permit_id, "
                    + "Permits.vehicle_id as vehicle_id, "
                    + "Permits.time as reg_time, "
                    + "Vehicles.model, "
                    + "Vehicles.manufacturer, "
                    + "Vehicles.prod_year as year, "
                    + "Users.FirstName as first_name, "
                    + "Users.LastName as last_name " 
                    + "FROM "
                    + "Permits "
                    + "INNER JOIN Vehicles ON Vehicles.id = Permits.vehicle_id AND Permits.status = @status "
                    + "INNER JOIN Users ON Users.id = @user_id AND Users.id = Permits.user_id  ";
            using (SqlConnection conn = new SqlConnection(_conn_str)) {
                conn.Open();
                SqlCommand command = new SqlCommand(get_permits, conn);
                command.Parameters.AddWithValue("@status", status);
                command.Parameters.AddWithValue("@user_id", user_id);
                //command.Parameters.AddWithValue("@limit",limit);
                using (SqlDataReader reader = command.ExecuteReader()) {               
                    while (reader.Read()) {
                        permits.Add(new Permit(reader));
                    }  
                }
                conn.Close();
            }
            return permits;
        }
        private static string _conn_str = System.Environment.GetEnvironmentVariable("sqldb_connection");
        private class PermitList {
            List<Permit> waiting;
            List<Permit> approved;

            public PermitList(List<Permit> waiting, List<Permit> approved) {
                this.waiting = waiting;
                this.approved = approved;
            }
        }
    }
    public class Permit {
        int permit_id;
        int vehicle_id;
        string vehicle;
        string user_name;
        DateTime reg_time;

        public Permit(SqlDataReader reader) { 
            this.permit_id = (int)reader["permit_id"];
            this.vehicle_id = (int)reader["vehicle_id"];
            this.vehicle =(string)reader["manufactureer"] + " " + (int)reader["model"] + " " + (int)reader["year"];
            this.user_name = (string)reader["first_name"] + " " + (string)reader["last_name"];
            this.reg_time = (DateTime)reader["reg_time"];
        }
    }
}
