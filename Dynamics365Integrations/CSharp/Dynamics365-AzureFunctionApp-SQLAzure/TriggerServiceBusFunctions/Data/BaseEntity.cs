using Microsoft.Xrm.Sdk;

namespace TriggerServiceBusFunction
{
    public abstract class BaseEntity
    {
        public abstract object TransformEntity(Entity entobj);

        public string EntityName { get; set; }

        private bool IsAttributeExist(Entity entity, string value)
        {
            if (entity.GetAttributeValue<string>(value) != null)
            {
                return true;
            }

            return false;
        }        

        public EntityProp AssignValues(Entity entity, string value)
        {
            EntityProp entprop = new EntityProp();
            bool attribexist = IsAttributeExist(entity, value);
            
            entprop.Value = attribexist == true ? entity.GetAttributeValue<string>(value) : null;
            entprop.IsEmpty = attribexist;

            return entprop;
        }
    }

    public struct EntityProp
    {
        public bool IsEmpty { get; set; }

        public string Value { get; set; }
    }
}