

namespace Microsoft.Dynamics365.AppSource.Workflow
{
    using System;
    using System.Net.Http;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using Microsoft.Dynamics365.AppSource.Workflow.Logging;
    using System.Collections.Generic;

    class CheckLicense : ILicenseInfo
    {
        private IOrganizationService _service;

        public CheckLicense(IOrganizationService service)
        {
            _service = service;
        }

        private string RetrieveLicenseInfo()
        {
            QueryExpression licenseentityquery = new QueryExpression("cxp_appicationlicense");
            licenseentityquery.ColumnSet.AddColumns("cxp_licensedetails", "cxp_licensename");

            licenseentityquery.Criteria = new FilterExpression();
            licenseentityquery.Criteria.AddCondition("cxp_licensename", ConditionOperator.Equal, "LicenseInfo");

            EntityCollection _licenseEntity = _service.RetrieveMultiple(licenseentityquery);
            string attributelicensedetails = _licenseEntity.Entities[0].Attributes["cxp_licensedetails"].ToString();
            //string attributelicensename = _licenseEntity.Entities[0].Attributes["cxp_licensename"].ToString();

            //foreach (var a in _licenseEntity.Entities)
            //{
            //    tracingService.Trace("MyPlugin: {0}", ("Name: " + a["cxp_licensedetails"] + " " + a.Attributes["cxp_licensename"]));
            //}

            Logger.LogInstance.LogWarning(nameof(RetrieveLicenseInfo), "XML Is empty ?", attributelicensedetails.Trim());

            return attributelicensedetails.Trim();
        }

        public bool ValidateLicenseSignature(Entity entity)
        {
            string LicenseXml = entity["cxp_licensedetails"].ToString();
            Logger.LogInstance.LogWarning(nameof(ValidateLicenseSignature), "XML Is empty ?", LicenseXml);

            Dictionary<string, string> webResourcecollection = QueryHelper.ProcessWebResource(_service);
            string url = string.Empty;
            webResourcecollection.TryGetValue("FunctionAppURL", out url);
            string response = CallAzureFunctionApp(LicenseXml, url);
            Logger.LogInstance.LogWarning(nameof(ValidateLicenseSignature), "CallAzureFunctionApp response", response);

            if (response.Trim().ToLower() == "true")
            {
                return true;
            }
            else
                return false;
        }

        private string CallAzureFunctionApp(string xml, string Url)
        {
            string xmldoc = String.Empty;
            try
            {
                HttpClient Client = new HttpClient();
                StringContent queryString = new StringContent(xml);
                var response = Client.PostAsync(Url, queryString).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    xmldoc = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }
                else
                {
                    Logger.LogInstance.LogWarning(nameof(CallAzureFunctionApp), "Http Reponse Code", response.IsSuccessStatusCode.ToString());
                    throw new Exception($"Request to function app was not successfull : {response.IsSuccessStatusCode}");
                }
                return xmldoc;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}

