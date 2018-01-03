using Microsoft.Xrm.Sdk;

namespace TriggerServiceBusFunction
{
    public class TransformLeadEntity : BaseEntity
    {
        public override object TransformEntity(Entity entity)
        {
            var leadentity = new LeadEntity();
            leadentity.LeadId =   entity.Attributes["leadid"].ToString();
            leadentity.FirstName = AssignValues(entity, "firstname");
            leadentity.LastName = AssignValues(entity, "lastname"); 
            leadentity.FullName = AssignValues(entity, "fullname");
            leadentity.Subject = AssignValues(entity, "subject");            

            base.EntityName = entity.LogicalName;

            return leadentity;
        }

       
    }
  
    public class LeadEntity 
    {
        public string LeadId { get; set; }
        public EntityProp FirstName { get; set; }
        public EntityProp LastName { get; set; }
        public EntityProp FullName { get; set; }
        public EntityProp Subject { get; set; }

    }   
}
