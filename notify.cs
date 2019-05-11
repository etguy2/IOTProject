using System;
using System.Linq;
using System.Net;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Collections.Generic;

namespace CarSharing.notify
{
    public static class notify
    {
        [FunctionName("notify")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            //sendPush("Yo");
            string title = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "title", true) == 0)
                .Value;
            string body = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "body", true) == 0)
                .Value;
            return req.CreateResponse(HttpStatusCode.OK, sendPush(title, body));
        }
        private static readonly HttpClient client = new HttpClient();
        public static string sendPush(string title, string body)
        {
 
            var SERVER_API_KEY = System.Environment.GetEnvironmentVariable("server_api_key");
            var SENDER_ID = System.Environment.GetEnvironmentVariable("sender_id");
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://fcm.googleapis.com/fcm/send");
            httpWebRequest.ContentType = "application/json";
            
            httpWebRequest.Method = "POST";
            httpWebRequest.Headers.Add(string.Format("Authorization: key={0}", SERVER_API_KEY));
            httpWebRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));
            notification_data nd = new notification_data();
            nd.to = "c9-5Opvw-FU:APA91bFV7GbXMVCkPD-4dABRED3fFmpGj-gpEyAPEb2WefQEX6fO1xQ_PaMexKwRHA4huZ-pvZlpSRjA8PLcn43sgoTey1yDJNoVnjt9u7JFmuEuRocZYTnoTtuLYkgUFAHZL9t-Jp9X";
            nd.notification.title = title;
            nd.notification.body = body;
            nd.data.action = 1;
           
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
    }
    
}
