using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Dynamics365.AppSource.Workflow.Logging;

namespace Microsoft.Dynamics365.AppSource.Workflow
{
    public class QueryHelper
    {
        private static EntityCollection GetEntityCollection(IOrganizationService service, string entityName, string attributesName, string attributeValue, ColumnSet cols)
        {
            QueryExpression query = new QueryExpression
            {
                EntityName = entityName,
                ColumnSet = cols,
                Criteria = new FilterExpression
                {
                    Conditions ={
                                    new ConditionExpression
                                    {
                                        AttributeName = attributesName,                                        
                                        Operator = ConditionOperator.Equal, Values = { attributeValue }
                                    }
                                }
                }
            };
            return service.RetrieveMultiple(query);
        }

        
        private static string ConvertContentToXMLString(EntityCollection entityCollection)
        {
            string webResourceContent = String.Empty;
            var entity = entityCollection.Entities[0];
            if (entity.Attributes.Contains("content"))
            {
                webResourceContent = entity.Attributes["content"].ToString();
                byte[] binary = Convert.FromBase64String(webResourceContent);
                webResourceContent = UnicodeEncoding.UTF8.GetString(binary);
            }

            Logger.LogInstance.LogWarning(nameof(webResourceContent), "WebResource Content", webResourceContent);

            return webResourceContent;
        }

        private static Dictionary<string,string> ExtractXmlNodes(string webResourceContent)
        {
            //string nodePath = "/root/data[@name='FunctionAppURL']";
            string nodePath = "/root/data";
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(webResourceContent);
            XmlNodeList xnList = xml.SelectNodes(nodePath);
            string value = string.Empty;            
            Dictionary<string, string> xmldictionary = new Dictionary<string, string>();

            if (xnList.Count > 0)
            {   
                foreach (XmlNode xn in xnList)
                {
                    Logger.LogInstance.LogWarning(nameof(ExtractXmlNodes), "Function App URL", "Key : " + xn.Attributes["name"].Value + $"--  Value {xn.InnerText}");
                    xmldictionary.Add(xn.Attributes["name"].Value, xn.InnerText);
                }                
            }

            return xmldictionary;
        }

        public static Dictionary<string,string> ProcessWebResource(IOrganizationService organizationService)
        {
            try
            {   
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                EntityCollection webresourceEntity = QueryHelper.GetEntityCollection(organizationService, "webresource", "name", "cxp_Settings", new ColumnSet("name", "content"));
                if (webresourceEntity == null)
                    throw new ArgumentNullException("Entity collection is empty");

                string webResourcexml = ConvertContentToXMLString(webresourceEntity);
                dictionary = ExtractXmlNodes(webResourcexml);
                return dictionary;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
