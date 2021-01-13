using System.Collections.Generic;

namespace CrmNx.Xrm.Toolkit.Infrastructure
{
    public interface IWebApiFunction
    {
        EntityReference BoundEntity { get; }

        string RequestName { get; }

        IDictionary<string, object> Parameters { get; }
    }
}