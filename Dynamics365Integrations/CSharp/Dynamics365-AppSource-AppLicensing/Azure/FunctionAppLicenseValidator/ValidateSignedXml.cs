using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;

namespace Microsoft.Dynamics365.AppSource
{
    public static class ValidateSignedXml
    {
        [FunctionName("ValidateSignedXml")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
            string Licensedetails = await req.Content.ReadAsStringAsync();

            if (Licensedetails == null)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, new
                {
                    error = "Please post the License XML in the request body"
                });
            }
            else
            {
                try
                {
                    XmlSignature xmlSignature = new XmlSignature(log);
                    bool IsValid = xmlSignature.StartLicensingProcess(Licensedetails);
                    log.Info("IsValid " + IsValid);
                    return req.CreateResponse(HttpStatusCode.OK, IsValid);
                }
                catch (Exception ex)
                {
                    return req.CreateResponse(HttpStatusCode.BadRequest, new
                    {
                        error = $"Error parsing jsondocument {ex.Message}"
                    });
                }
            }
        }
    }
}
