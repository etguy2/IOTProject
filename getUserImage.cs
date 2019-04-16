using System.Net;
using System;
using System.Linq;
using System.Net;
using Microsoft.AspNet.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http.Headers;
using System.Drawing;
using System.IO;
using System.Data.SqlClient;



namespace CarSharing.GetUserImage
{
    public static class getUserImage
    {
        [FunctionName("getUserImage")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {            
            string user_id = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "user_id", true) == 0)
                .Value;
            
            // Task vars 
            string urlGetImage = "https://cdn.pixabay.com/photo/2015/10/05/22/37/blank-profile-picture-973460_960_720.png";
            string _conn_str = System.Environment.GetEnvironmentVariable("sqldb_connection");
            string query =  "SELECT top 1 profile_image FROM Users WHERE id = "+user_id;

            using (SqlConnection conn = new SqlConnection(_conn_str)) {
                conn.Open();
                SqlCommand get_cmd = new SqlCommand(query, conn);
                using (SqlDataReader reader = get_cmd.ExecuteReader()) {
                    if (reader.Read()) 
                        urlGetImage = (string)reader["profile_image"];
                }
                conn.Close();
            }

            WebClient wc = new WebClient();
            byte[] bytes = wc.DownloadData(urlGetImage);
            MemoryStream ms = new MemoryStream(bytes);
            Image img = utilitles.CropImage(Image.FromStream(ms));
            ImageConverter converter = new ImageConverter();
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(ImageToByteArray(img));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            return response;
        }
        private static byte[] ImageToByteArray(Image image)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(image, typeof(byte[]));
        }
    }
}
