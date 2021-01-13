using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CrmNx.Xrm.Toolkit.Messages
{
    public class QueryScheduleResponse
    {
        public QueryScheduleResponse(IList<TimeInfo> timeInfos)
        {
            TimeInfos = new Collection<TimeInfo>(timeInfos);
        }

        public Collection<TimeInfo> TimeInfos { get; }
    }
}