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

namespace CarSharing.addVehicle
{
    public static class addVehicle
    {
        [FunctionName("addVehicle")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            response response = new response(0, "Error");
            try {
                // parse query parameter
                int user_id = Convert.ToInt32(utilitles.getURLVar(req, "user_id"));
                string login_hash = utilitles.getURLVar(req, "login_hash");

                string manufacturer = utilitles.getURLVar(req, "manufacturer");
                int prod_year = Convert.ToInt32(utilitles.getURLVar(req, "prod_year"));
                int id = Convert.ToInt32(utilitles.getURLVar(req, "plate"));
                int mode = Convert.ToInt32(utilitles.getURLVar(req, "mode"));
                string model = utilitles.getURLVar(req, "model");


                // Validates user identity.
                utilitles.validateUser( user_id , login_hash );

                response.status = 1;
                response.description = "Vehicle Added";
            } catch (CarSharingException ex) {
                response = new response(ex.status_code, "Error: " + ex.info);
            }
            return req.CreateResponse(HttpStatusCode.OK, response, JsonMediaTypeFormatter.DefaultMediaType);
        }
    }
}
