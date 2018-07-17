# Step By Step : Build and Deployment

## Code Structure
![](https://github.com/venkatramanak/CPS/blob/users/kvramana/License/Dynamics365Integrations/CSharp/Dynamics365-AppSource-AppLicensing/Images/ProjectStructureDefintion.PNG)

***

## Technology Stack Used

### Azure Services [Function App and KeyVault]

* Azure Function : Build and deploy from Visual studio into your Azure Instance. Learn deploying Azure Function using visuale studio https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-vs
	1. Please ensure Azure function is running.
	2. Validate the Azure function by passing the dummy data or Signed XML.

* Dynamics365 : Build and deploy code using Plug-In Registration Tool.
		
	* Import Solution
		1. "AppSourceAppLicensingSolution" (LicensingEntity, WebResource and Workflow)
	
	Code Modification and deploy the code
	
	* Customer Activity Workflow deployment
		1. Build WorkflowActivityLicenseValidator.csproj and deploy into Dynamics365 using PlugIn registration tool.

	* Plug-In deployment
		1. Build PlugInLicenseExpiryValidator.csproj and deploy into Dynamics365 using PlugIn registration tool.