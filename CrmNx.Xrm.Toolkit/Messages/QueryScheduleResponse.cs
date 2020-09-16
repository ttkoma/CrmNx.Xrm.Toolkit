using System.Collections.ObjectModel;

namespace CrmNx.Xrm.Toolkit.Messages
{
    public class QueryScheduleResponse
    {
        public Collection<TimeInfo> TimeInfos { get; }

        public QueryScheduleResponse(TimeInfo[] timeInfos)
        {
            TimeInfos = new Collection<TimeInfo>(timeInfos);
        }
    }
}
