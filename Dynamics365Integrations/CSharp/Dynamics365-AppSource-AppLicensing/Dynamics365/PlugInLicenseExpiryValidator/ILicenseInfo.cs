
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Security.Permissions;
using System.Xml;
using System.Net.Http;
using System.Collections.Generic;

using Microsoft.Dynamics365.AppSource.PlugIn.Logging;
using Microsoft.Dynamics365.AppSource.PlugIn.Helper;

namespace Microsoft.Dynamics365.AppSource.PlugIn
{
    interface ILicenseInfo
    {    
        bool ValidateLicenseSignature(Entity entity);
        bool ValidateLicenseExpiry();
    }

    
    class LicenseInfo : ILicenseInfo
    {        
        private readonly IOrganizationService _service;        

        public LicenseInfo (IOrganizationService service)
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
            string attributelicensedetails = _licenseEntity?.Entities[0]?.Attributes["cxp_licensedetails"].ToString();
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

            Dictionary<string,string> webResourcecollection = QueryHelper.ProcessWebResource(_service);
            string url = string.Empty;
            webResourcecollection.TryGetValue("FunctionAppURL", out url);
            string response = CallAzureFunctionApp(LicenseXml, url);
            Logger.LogInstance.LogWarning(nameof(ValidateLicenseSignature), "CallAzureFunctionApp response", response);
                        
            if(response.Trim().ToLower() == "true")
            {
                return true;
            }
            else
                return false;
        }

        public bool ValidateLicenseExpiry()
        {
            string LicenseXml = RetrieveLicenseInfo();                                    

            DateTime currentDate = DateTime.UtcNow;
            DateTime expiryDate;
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(LicenseXml);

            XmlNodeList nodeList = xmldoc.GetElementsByTagName("expiry");
            expiryDate = Convert.ToDateTime(nodeList[0]?.InnerText);
            Logger.LogInstance.LogWarning(nameof(ValidateLicenseExpiry), "Expiry date check", $"Xml current date : {currentDate} expiry date : {expiryDate}");         
            
            int result = DateTime.Compare(expiryDate, currentDate);
            Logger.LogInstance.LogWarning(nameof(ValidateLicenseExpiry), "Date Compare", result.ToString());
            if (result == -1)
                return true;
            else
                return false;
        }

        private string CallAzureFunctionApp(string xml,string Url)
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