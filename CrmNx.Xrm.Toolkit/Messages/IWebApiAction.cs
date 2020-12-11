using System.Collections.Generic;

namespace CrmNx.Xrm.Toolkit.Messages
{
    public interface IWebApiAction
    {
        EntityReference BoundEntity { get; }
        
        string Action { get; }
        
        IDictionary<string, object> Parameters { get;  }
    }
}