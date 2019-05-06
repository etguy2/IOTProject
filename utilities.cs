
using System;   
using System.IO;
using System.Net;   
using System.Text;  
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Data.SqlClient;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using CarSharing.Cryptography;

public static class utilitles {
    private static Random random = new Random();
    private static string _conn_str = System.Environment.GetEnvironmentVariable("sqldb_connection");
    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
        .Select(s => s[random.Next(s.Length)]).ToArray());
    }
    public static bool isKeyExist(SqlDataReader reader, string columnName) {
        for (int i=0; i < reader.FieldCount; i++)
        {
            
            if (reader.GetName(i).Equals(columnName,StringComparison.InvariantCultureIgnoreCase))
                return true;
        }
        return false; 
    }

    public static string safeCast(SqlDataReader reader, string slot_name) {
        return (utilitles.isKeyExist(reader, slot_name) && reader[slot_name] != DBNull.Value) ? (string)reader[slot_name] : string.Empty;
    }
    public static Image CropImage(Image img) {
        int x = img.Width/2;
        int y = img.Height/2;
        int r = Math.Min(x, y);
        Bitmap tmp = null;
        tmp = new Bitmap(2*r, 2*r);
        using (Graphics g = Graphics.FromImage(tmp)) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TranslateTransform(tmp.Width / 2, tmp.Height / 2);
            GraphicsPath gp = new GraphicsPath();
            gp.AddEllipse(0-r, 0-r, 2*r, 2*r);
            Region rg = new Region(gp);
            g.SetClip(rg, CombineMode.Replace);
            Bitmap bmp = new Bitmap(img);
            g.DrawImage(bmp, new Rectangle(-r, -r, 2*r, 2*r), new Rectangle(x-y, y-r, 2*r, 2*r), GraphicsUnit.Pixel);
        }
        return tmp;
    }

    public static bool validateUser(int user_id, string login_hash) {
        string validate_query = "SELECT password_enc, enc_string FROM Users WHERE id = @user_id";
        string _conn_str = System.Environment.GetEnvironmentVariable("sqldb_connection");
        bool status = false;
        using (SqlConnection conn = new SqlConnection(_conn_str)) {
            conn.Open();
            SqlCommand command = new SqlCommand(validate_query, conn);
            command.Parameters.AddWithValue("@user_id", user_id);
            using (SqlDataReader reader = command.ExecuteReader()) {               
                if (reader.Read()) {
                    string stored_login_hash = SHA.GenerateSHA256String((string)reader["password_enc"]+(string)reader["enc_string"]).ToLower();
                    if (stored_login_hash == login_hash)
                        status = true;
                }
            }
            conn.Close();
        }
        return status;
    }
        public static string notifyUserById(string title, string body, int user_id) {
            string validate_query = "SELECT notification_token FROM Users WHERE id = @user_id";
            
            using (SqlConnection conn = new SqlConnection(_conn_str)) {
                conn.Open();
                SqlCommand command = new SqlCommand(validate_query, conn);
                command.Parameters.AddWithValue("@user_id", user_id);
                using (SqlDataReader reader = command.ExecuteReader()) {               
                    if (reader.Read()) {
                        return sendPush(title, body, (string)reader["notification_token"]);
                    }
                }
                conn.Close();
            }
            return "";
        }
        private static string sendPush(string title, string body, string target)
        {
 
            var SERVER_API_KEY = System.Environment.GetEnvironmentVariable("server_api_key");
            var SENDER_ID = System.Environment.GetEnvironmentVariable("sender_id");
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://fcm.googleapis.com/fcm/send");
            httpWebRequest.ContentType = "application/json";
            
            httpWebRequest.Method = "POST";
            httpWebRequest.Headers.Add(string.Format("Authorization: key={0}", SERVER_API_KEY));
            httpWebRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));
            notification_data nd = new notification_data();
            nd.to = target;
            nd.notification.title = title;
            nd.notification.body = body;
            nd.data.action = 1;
            nd.data.message = "Inner Message";
            string postData = Newtonsoft.Json.JsonConvert.SerializeObject(nd);
            // string postData = "{\"collapse_key\":\"score_update\",\"time_to_live\":108,\"delay_while_idle\":true,\"data\": { \"message\" : \"{\"title\":\""+message+"\"}},\"time\": \"" + System.DateTime.Now.ToString() + "\"},\"registration_ids\":[\"c9-5Opvw-FU:APA91bFV7GbXMVCkPD-4dABRED3fFmpGj-gpEyAPEb2WefQEX6fO1xQ_PaMexKwRHA4huZ-pvZlpSRjA8PLcn43sgoTey1yDJNoVnjt9u7JFmuEuRocZYTnoTtuLYkgUFAHZL9t-Jp9X\"]}";
            //string postData = "{\"to\":\"c9-5Opvw-FU:APA91bFV7GbXMVCkPD-4dABRED3fFmpGj-gpEyAPEb2WefQEX6fO1xQ_PaMexKwRHA4huZ-pvZlpSRjA8PLcn43sgoTey1yDJNoVnjt9u7JFmuEuRocZYTnoTtuLYkgUFAHZL9t-Jp9X\", \"notification\":{\"title\":\""+title+"\", \"body\":\""+body+"\"}, \"data\":{\"message\":\"hello3\"}}";
            Byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            httpWebRequest.ContentLength = byteArray.Length;
    
            Stream dataStream = httpWebRequest.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
    
            WebResponse tResponse = httpWebRequest.GetResponse();
    
            dataStream = tResponse.GetResponseStream();
    
            StreamReader tReader = new StreamReader(dataStream);
    
            String sResponseFromServer = tReader.ReadToEnd();
    
            tReader.Close();
            dataStream.Close();
            tResponse.Close();
            return sResponseFromServer + " " + postData;
        }
        private static readonly HttpClient client = new HttpClient();
        public static string getURLVar(HttpRequestMessage req, string name) {
            return req.GetQueryNameValuePairs()
                    .FirstOrDefault(q => string.Compare(q.Key, name, true) == 0)
                    .Value;
        }

        public static int getOwnerByVehicle(string  vehicle_id) {
            using (SqlConnection conn = new SqlConnection(_conn_str)) {
                conn.Open();
                string get_user_query = "SELECT owner_id FROM Vehicles WHERE id = @vehicle_id";
                SqlCommand command = new SqlCommand(get_user_query, conn);
                command.Parameters.AddWithValue("@user_id", get_user_query);
                using (SqlDataReader reader = command.ExecuteReader()) {               
                    if (reader.Read()) {
                        conn.Close();
                        return (int)reader["owner_id"];
                    }
                }
                conn.Close();
                return 0;
                
            }
        }
}
