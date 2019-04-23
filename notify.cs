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
using System.Collections.Generic; 

namespace CarSharing.notify
{
    public static class notify
    {
        [FunctionName("notify")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            //sendPush("Yo");
            return req.CreateResponse(HttpStatusCode.OK, sendPush("Yo"));
        }
        private static readonly HttpClient client = new HttpClient();
        public static string sendPush(string message)
        {
 
            var SERVER_API_KEY = System.Environment.GetEnvironmentVariable("server_api_key");
            var SENDER_ID = System.Environment.GetEnvironmentVariable("sender_id");
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://fcm.googleapis.com/fcm/send");
            httpWebRequest.ContentType = "application/json";
            
            httpWebRequest.Method = "POST";
            httpWebRequest.Headers.Add(string.Format("Authorization: key={0}", SERVER_API_KEY));
            httpWebRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));
            string postData = "{\"collapse_key\":\"score_update\",\"time_to_live\":108,\"delay_while_idle\":true,\"data\": { \"message\" : \"{\"title\":\""+message+"\"}},\"time\": \"" + System.DateTime.Now.ToString() + "\"},\"registration_ids\":[\"c9-5Opvw-FU:APA91bFV7GbXMVCkPD-4dABRED3fFmpGj-gpEyAPEb2WefQEX6fO1xQ_PaMexKwRHA4huZ-pvZlpSRjA8PLcn43sgoTey1yDJNoVnjt9u7JFmuEuRocZYTnoTtuLYkgUFAHZL9t-Jp9X\"]}";
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
            return sResponseFromServer;
            
        }
        private class NotificationMessage
	    {
            public string Title;
            public string Message;
            public long ItemId;
	    }
    }
    
}
