using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http.Formatting;

namespace CarSharing.Function
{
    public static class isUserExist
    {
        [FunctionName("isUserExist")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            string name = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
                .Value;
            response res = new response(0, "No such user in database.");
            if (name == null) {
                res.setData(-1, "No user sent!");
            } else {
                dbConnect db = new dbConnect ();
                int rows = db.query_scalar("SELECT  COUNT(*) FROM Users WHERE FirstName = '" + name + "'");
                if (rows > 0) {
                    res.setData(1, "FOUND!");
                }
            }
            return name == null
                ? req.CreateResponse(HttpStatusCode.BadRequest, res, JsonMediaTypeFormatter.DefaultMediaType)
                : req.CreateResponse(HttpStatusCode.OK, res, JsonMediaTypeFormatter.DefaultMediaType);
        }
    }
}
