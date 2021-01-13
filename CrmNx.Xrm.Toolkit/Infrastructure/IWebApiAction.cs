using System.Collections.Generic;

namespace CrmNx.Xrm.Toolkit.Infrastructure
{
    public interface IWebApiAction
    {
        EntityReference BoundEntity { get; }
        
        string Action { get; }
        
        IDictionary<string, object> Parameters { get;  }
    }
}