
namespace Microsoft.Dynamics365.AppSource.Workflow
{
    using System;
    using System.Xml;
    using System.Net.Http;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    interface ILicenseInfo
    {
        bool ValidateLicenseSignature(Entity entity);
        
    }


  
}
