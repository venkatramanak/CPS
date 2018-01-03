using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using Microsoft.Xrm.Sdk;

namespace TriggerServiceBusFunction
{
    public static class funServiceBusQueueSQL
    {
        [FunctionName("funServiceBusQueueSQL")]
        public static void Run([ServiceBusTrigger("cpsisvsbsqueue", AccessRights.Listen, Connection = "AzureServiceBusConnection")]BrokeredMessage myQueueItem, TraceWriter log)
        {
            log.Info($"C# ServiceBus queue trigger function processed message: {myQueueItem}");


            RemoteExecutionContext contextFromJSON = myQueueItem.GetBody<RemoteExecutionContext>(
                new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(RemoteExecutionContext)));

            EntityImageCollection entityImageCollection = contextFromJSON.PostEntityImages;

            Entity entityobject = (Entity)contextFromJSON.InputParameters["Target"];

            BaseEntity baseentity = null;
            object _entity = null;            
            string entityPrimaryKeyColumn = string.Empty;

            switch (entityobject.LogicalName.ToLower())
            {
                case "lead":
                    baseentity = new TransformLeadEntity();
                    _entity = (LeadEntity)baseentity.TransformEntity(entityobject);                    
                    entityPrimaryKeyColumn = "LeadId";
                    break;
                default:                    
                    break;
            }
        
            
            SQLHelper sqlhelper = new SQLHelper(entityPrimaryKeyColumn);
            sqlhelper.Execute(_entity, baseentity.EntityName);


            //foreach (KeyValuePair<string, object> parameter in entityobject.Attributes)
            //{

            //    //Write code here to transform the data into SQL
            //}

            //EntityImageCollection entityImageCollection = contextFromJSON.PostEntityImages;

            //foreach (KeyValuePair<string, Entity> entityImage in entityImageCollection)
            //{             
            //    //Write code here to transform the data into SQL
            //}                

        }
    }

}
