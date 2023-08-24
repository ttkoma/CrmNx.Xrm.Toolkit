using CrmNx.Xrm.Toolkit;
using System.Collections.Generic;

namespace CrmNx.Xrm.Identity.Dto
{
    public class EntityIdComparer : IEqualityComparer<Entity>
    {
        public bool Equals(Entity x, Entity y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(Entity entity)
        {
            return entity.Id.GetHashCode();
        }
    }
}
