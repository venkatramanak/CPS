
namespace Microsoft.Dynamics365.AppSource.Workflow
{
    using System;
    using System.Activities;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Workflow;
    using System.ServiceModel;
    using Microsoft.Dynamics365.AppSource.Workflow.Logging;

    public class ExecuteLicenseWorkflow : CodeActivity
    {
        protected override void Execute(CodeActivityContext context)
        {
            //Create the tracing service            
            ITracingService tracingService = context.GetExtension<ITracingService>();
            //Create the context
            IWorkflowContext workflowcontext = context.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(workflowcontext.UserId);

            Logger.LogInstance.AssignTrace(tracingService);

            Entity entity = (Entity)workflowcontext.InputParameters["Target"];
            try
            {
                ILicenseInfo licenseInfo = new CheckLicense(service);
                tracingService.Trace($"Plug-In fired for entity: {entity.LogicalName}");
                bool IsValid = false;
                if (entity.LogicalName == "cxp_appicationlicense")
                {
                    IsValid = licenseInfo.ValidateLicenseSignature(entity);
                    tracingService.Trace($"ValidateLicenseSignature returns {IsValid}");
                    if (!IsValid)
                        throw new InvalidPluginExecutionException("Your License is not vaild, Please acquire a new license from Contoso Inc. to proceed further");
                }              

            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("An error occurred in MyPlug-in.", ex);
            }
            catch (Exception ex)
            {
                tracingService.Trace("MyPlugin: {0}", ex.ToString());
                throw;
            }

        }
    }
}
